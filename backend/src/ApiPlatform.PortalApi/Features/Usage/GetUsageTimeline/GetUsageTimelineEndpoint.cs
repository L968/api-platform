using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Features.Usage;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Usage.GetUsageTimeline;

public static class GetUsageTimelineEndpoint
{
    public static void MapGetUsageTimelineEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/usage/timeline", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        [AsParameters] GetUsageTimelineRequest request,
        PortalDbContext db)
    {
        if (!TryParseGranularity(request.Granularity, out TimelineGranularity granularity))
        {
            return Results.BadRequest("Granularity must be day, week or month.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly start = request.From ?? today.AddDays(-29);
        DateOnly end = request.To ?? today;

        if (start > end)
        {
            return Results.BadRequest("The start date must be before the end date.");
        }

        Guid organizationId = EndpointHelpers.OrganizationId(principal);
        IQueryable<ApiUsageDaily> query = db.ApiUsageDaily.Where(usage =>
            usage.OrganizationId == organizationId &&
            usage.Date >= start &&
            usage.Date <= end);

        if (request.ApplicationId.HasValue)
        {
            query = query.Where(usage => usage.ApplicationId == request.ApplicationId.Value);
        }

        List<UsageDailyData> usageItems = await query
            .Select(usage => new UsageDailyData(
                usage.ApiId,
                usage.Date,
                usage.RequestCount,
                usage.ErrorCount,
                usage.AvgLatencyMs))
            .ToListAsync();

        List<OrganizationApiPricing> prices = await db.OrganizationApiPricing
            .Where(pricing => pricing.OrganizationId == organizationId)
            .ToListAsync();
        var priceResolver = new UsagePriceResolver(prices);
        Guid[] apiIds = prices.Select(price => price.ApiId).Distinct().ToArray();
        Dictionary<Guid, string> apiNames = await db.Apis
            .Where(api => apiIds.Contains(api.Id))
            .ToDictionaryAsync(api => api.Id, api => api.Name);

        var usageByPeriod = usageItems
            .GroupBy(usage => PeriodStart(usage.Date, granularity))
            .ToDictionary(group => group.Key, group => group.ToList());

        var points = new List<GetUsageTimelinePointResponse>();
        DateOnly period = PeriodStart(start, granularity);
        while (period <= end)
        {
            points.Add(usageByPeriod.TryGetValue(period, out List<UsageDailyData>? values)
                ? CreatePoint(period, values, priceResolver)
                : new GetUsageTimelinePointResponse(period, 0, 0, 0, 0));

            period = NextPeriod(period, granularity);
        }

        return Results.Ok(new GetUsageTimelineResponse(
            start,
            end,
            granularity.ToString().ToLowerInvariant(),
            points,
            CreatePricingChanges(prices, apiNames, start, end)));
    }

    private static List<GetUsagePricingChangeResponse> CreatePricingChanges(
        IEnumerable<OrganizationApiPricing> prices,
        IReadOnlyDictionary<Guid, string> apiNames,
        DateOnly start,
        DateOnly end)
    {
        return prices
            .Where(price => price.EffectiveFrom >= start && price.EffectiveFrom <= end)
            .GroupBy(price => price.ApiId)
            .SelectMany(group => group
                .OrderBy(price => price.EffectiveFrom)
                .Select((price, index) => new GetUsagePricingChangeResponse(
                    apiNames.GetValueOrDefault(price.ApiId, "API"),
                    price.EffectiveFrom,
                    index == 0
                        ? prices
                            .Where(previous => previous.ApiId == price.ApiId && previous.EffectiveFrom < price.EffectiveFrom)
                            .OrderByDescending(previous => previous.EffectiveFrom)
                            .Select(previous => (decimal?)previous.PricePerRequest)
                            .FirstOrDefault()
                        : group
                            .Where(previous => previous.EffectiveFrom < price.EffectiveFrom)
                            .OrderByDescending(previous => previous.EffectiveFrom)
                            .Select(previous => (decimal?)previous.PricePerRequest)
                            .FirstOrDefault(),
                    price.PricePerRequest)))
            .Where(change => change.PreviousPricePerRequest.HasValue)
            .OrderBy(change => change.EffectiveFrom)
            .ThenBy(change => change.Api)
            .ToList();
    }

    private static GetUsageTimelinePointResponse CreatePoint(
        DateOnly periodStart,
        IEnumerable<UsageDailyData> items,
        UsagePriceResolver priceResolver)
    {
        var values = items.ToList();
        int requests = values.Sum(item => item.Requests);
        double averageLatency = requests == 0
            ? 0
            : values.Sum(item => item.AverageLatencyMs * item.Requests) / requests;
        decimal cost = values.Sum(item =>
            Math.Max(0, item.Requests - item.Errors) * priceResolver.PriceAt(item.ApiId, item.Date));

        return new GetUsageTimelinePointResponse(
            periodStart,
            requests,
            values.Sum(item => item.Errors),
            averageLatency,
            cost);
    }

    private static DateOnly PeriodStart(DateOnly date, TimelineGranularity granularity)
    {
        if (granularity == TimelineGranularity.Month)
        {
            return new DateOnly(date.Year, date.Month, 1);
        }

        if (granularity == TimelineGranularity.Week)
        {
            int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
            return date.AddDays(-daysSinceMonday);
        }

        return date;
    }

    private static DateOnly NextPeriod(
        DateOnly period,
        TimelineGranularity granularity)
    {
        if (granularity == TimelineGranularity.Month)
        {
            return period.AddMonths(1);
        }

        return period.AddDays(granularity == TimelineGranularity.Week ? 7 : 1);
    }

    private static bool TryParseGranularity(
        string? value,
        out TimelineGranularity granularity)
    {
        return Enum.TryParse(value ?? "day", true, out granularity) &&
            Enum.IsDefined(granularity);
    }

    private enum TimelineGranularity
    {
        Day,
        Week,
        Month
    }

    private sealed record UsageDailyData(
        Guid ApiId,
        DateOnly Date,
        int Requests,
        int Errors,
        int AverageLatencyMs);
}
