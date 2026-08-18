using System.Threading.Channels;
using Npgsql;

namespace ApiPlatform.Gateway.Metering;

public sealed class UsageAggregationWorker : BackgroundService, IUsageSink
{
    private const string UpsertSql = """
        INSERT INTO api_usage_daily
            (id, organization_id, application_id, api_id, endpoint, date,
             request_count, error_count, avg_latency_ms)
        VALUES
            ($1, $2, $3, $4, $5, $6, $7, $8, $9)
        ON CONFLICT (organization_id, application_id, api_id, endpoint, date)
        DO UPDATE SET
            error_count = api_usage_daily.error_count + EXCLUDED.error_count,
            avg_latency_ms = (
                (
                    api_usage_daily.avg_latency_ms::bigint * api_usage_daily.request_count +
                    EXCLUDED.avg_latency_ms::bigint * EXCLUDED.request_count
                ) / (api_usage_daily.request_count + EXCLUDED.request_count)
            )::integer,
            request_count = api_usage_daily.request_count + EXCLUDED.request_count;
        """;

    private readonly Channel<UsageEvent> _channel;
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<UsageAggregationWorker> _logger;
    private readonly TimeSpan _flushInterval;
    private readonly Dictionary<AggregateKey, Aggregate> _aggregates = [];
    private long _droppedEvents;

    public UsageAggregationWorker(
        NpgsqlDataSource dataSource,
        ILogger<UsageAggregationWorker> logger,
        IConfiguration configuration)
    {
        _dataSource = dataSource;
        _logger = logger;

        int queueCapacity = PositiveSetting(configuration, "UsageMeter:QueueCapacity", 10_000);
        int flushIntervalSeconds = PositiveSetting(configuration, "UsageMeter:FlushIntervalSeconds", 5);
        _flushInterval = TimeSpan.FromSeconds(flushIntervalSeconds);

        _channel = Channel.CreateBounded<UsageEvent>(new BoundedChannelOptions(queueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public bool TryWrite(UsageEvent usageEvent)
    {
        bool written = _channel.Writer.TryWrite(usageEvent);
        if (!written)
        {
            Interlocked.Increment(ref _droppedEvents);
        }

        return written;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_flushInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            DrainQueue();
            await Flush(stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        DrainQueue();
        await Flush(cancellationToken);
    }

    private void DrainQueue()
    {
        while (_channel.Reader.TryRead(out UsageEvent? usageEvent))
        {
            AggregateKey key = new(
                usageEvent.OrganizationId,
                usageEvent.ApplicationId,
                usageEvent.ApiId,
                usageEvent.Endpoint,
                usageEvent.Date);

            if (!_aggregates.TryGetValue(key, out Aggregate? aggregate))
            {
                aggregate = new Aggregate();
                _aggregates.Add(key, aggregate);
            }

            aggregate.Add(usageEvent);
        }

        long droppedEvents = Interlocked.Exchange(ref _droppedEvents, 0);
        if (droppedEvents > 0)
        {
            _logger.LogWarning(
                "Usage meter descartou {DroppedEvents} eventos porque a fila estava cheia.",
                droppedEvents);
        }
    }

    private async Task Flush(CancellationToken cancellationToken)
    {
        if (_aggregates.Count == 0)
        {
            return;
        }

        try
        {
            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

            foreach ((AggregateKey key, Aggregate aggregate) in _aggregates)
            {
                await Upsert(connection, transaction, key, aggregate, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            _aggregates.Clear();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Falha ao persistir agregados de consumo; o lote será tentado novamente.");
        }
    }

    private static async Task Upsert(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        AggregateKey key,
        Aggregate aggregate,
        CancellationToken cancellationToken)
    {
        await using NpgsqlCommand command = new(UpsertSql, connection, transaction);
        command.Parameters.AddWithValue(Guid.CreateVersion7());
        command.Parameters.AddWithValue(key.OrganizationId);
        command.Parameters.AddWithValue(key.ApplicationId);
        command.Parameters.AddWithValue(key.ApiId);
        command.Parameters.AddWithValue(key.Endpoint);
        command.Parameters.AddWithValue(key.Date);
        command.Parameters.AddWithValue(aggregate.RequestCount);
        command.Parameters.AddWithValue(aggregate.ErrorCount);
        command.Parameters.AddWithValue(aggregate.AverageLatencyMs);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static int PositiveSetting(
        IConfiguration configuration,
        string key,
        int defaultValue)
    {
        int value = configuration.GetValue(key, defaultValue);
        if (value <= 0)
        {
            throw new InvalidOperationException($"{key} deve ser maior que zero.");
        }

        return value;
    }

    private sealed record AggregateKey(
        Guid OrganizationId,
        Guid ApplicationId,
        Guid ApiId,
        string Endpoint,
        DateOnly Date);

    private sealed class Aggregate
    {
        public int RequestCount { get; private set; }
        public int ErrorCount { get; private set; }
        public long TotalLatencyMs { get; private set; }

        public int AverageLatencyMs => RequestCount == 0
            ? 0
            : (int)(TotalLatencyMs / RequestCount);

        public void Add(UsageEvent usageEvent)
        {
            RequestCount++;
            TotalLatencyMs += usageEvent.LatencyMs;

            if (usageEvent.IsError)
            {
                ErrorCount++;
            }
        }
    }
}
