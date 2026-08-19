using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Usage;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Billing;

public sealed class InvoiceGenerationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InvoiceGenerationWorker> _logger;

    public InvoiceGenerationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<InvoiceGenerationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await GeneratePreviousMonthInvoices(stoppingToken);

        using PeriodicTimer timer = new(TimeSpan.FromDays(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await GeneratePreviousMonthInvoices(stoppingToken);
        }
    }

    private async Task GeneratePreviousMonthInvoices(CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            PortalDbContext db = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currentMonth = new DateOnly(today.Year, today.Month, 1);
            DateOnly periodStart = currentMonth.AddMonths(-1);
            DateOnly periodEnd = currentMonth.AddDays(-1);
            List<Guid> organizationIds = await db.Organizations
                .Where(organization => organization.Status == OrganizationStatus.Active)
                .Select(organization => organization.Id)
                .ToListAsync(cancellationToken);

            foreach (Guid organizationId in organizationIds)
            {
                await GenerateInvoice(
                    db,
                    organizationId,
                    periodStart,
                    periodEnd,
                    cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to generate invoices for the completed period.");
        }
    }

    private static async Task GenerateInvoice(
        PortalDbContext db,
        Guid organizationId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        bool alreadyExists = await db.Invoices.AnyAsync(invoice =>
            invoice.OrganizationId == organizationId &&
            invoice.PeriodStart == periodStart &&
            invoice.PeriodEnd == periodEnd,
            cancellationToken);
        if (alreadyExists)
        {
            return;
        }

        List<UsageSnapshot> usage = await db.ApiUsageDaily
            .Where(item =>
                item.OrganizationId == organizationId &&
                item.Date >= periodStart &&
                item.Date <= periodEnd)
            .Select(item => new UsageSnapshot(
                item.ApiId,
                item.Endpoint,
                item.Date,
                item.RequestCount,
                item.ErrorCount))
            .ToListAsync(cancellationToken);
        List<OrganizationApiPricing> prices = await db.OrganizationApiPricing
            .Where(price => price.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
        Dictionary<Guid, string> apiNames = await db.Apis
            .ToDictionaryAsync(api => api.Id, api => api.Name, cancellationToken);
        var resolver = new UsagePriceResolver(prices);
        var lineValues = usage
            .GroupBy(item => new
            {
                item.ApiId,
                item.Endpoint,
                Price = resolver.FindAt(item.ApiId, item.Date)
            })
            .Where(group => group.Key.Price is not null)
            .Select(group => CreateLineValues(
                group.Key.ApiId,
                apiNames.GetValueOrDefault(group.Key.ApiId, "API"),
                group.Key.Endpoint,
                group,
                group.Key.Price!))
            .ToList();
        decimal total = lineValues.Sum(line => line.Amount);
        DateTime issuedAt = DateTime.UtcNow;
        var dueAt = DateTime.SpecifyKind(
            periodEnd.AddDays(15).ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc);
        var invoice = new Invoice(
            organizationId,
            $"INV-{periodStart:yyyy-MM}",
            periodStart,
            periodEnd,
            total,
            issuedAt,
            dueAt);

        foreach (LineValues line in lineValues)
        {
            invoice.Lines.Add(new InvoiceLine(
                invoice.Id,
                line.ApiId,
                line.Api,
                line.Endpoint,
                line.Requests,
                line.Errors,
                line.BillableRequests,
                line.PricePerRequest,
                line.PriceEffectiveFrom,
                line.Amount));
        }

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static LineValues CreateLineValues(
        Guid apiId,
        string api,
        string endpoint,
        IEnumerable<UsageSnapshot> usage,
        OrganizationApiPricing price)
    {
        var values = usage.ToList();
        int requests = values.Sum(item => item.Requests);
        int errors = values.Sum(item => item.Errors);
        int billableRequests = values.Sum(item => Math.Max(0, item.Requests - item.Errors));
        decimal amount = billableRequests * price.PricePerRequest;

        return new LineValues(
            apiId,
            api,
            endpoint,
            requests,
            errors,
            billableRequests,
            price.PricePerRequest,
            price.EffectiveFrom,
            amount);
    }

    private sealed record UsageSnapshot(
        Guid ApiId,
        string Endpoint,
        DateOnly Date,
        int Requests,
        int Errors);

    private sealed record LineValues(
        Guid ApiId,
        string Api,
        string Endpoint,
        int Requests,
        int Errors,
        int BillableRequests,
        decimal PricePerRequest,
        DateOnly PriceEffectiveFrom,
        decimal Amount);
}
