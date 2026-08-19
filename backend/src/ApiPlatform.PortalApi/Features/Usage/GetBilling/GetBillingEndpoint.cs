using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Usage;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Usage.GetBilling;

public static class GetBillingEndpoint
{
    public static void MapGetBillingEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/billing", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, [AsParameters] GetBillingRequest request, PortalDbContext db)
    {
        Guid organizationId = EndpointHelpers.OrganizationId(principal);
        DateOnly start = request.From ?? new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        DateOnly end = request.To ?? DateOnly.FromDateTime(DateTime.UtcNow);

        IQueryable<ApiPlatform.PortalApi.Domain.Entities.ApiUsageDaily> usageQuery = db.ApiUsageDaily.Where(usage =>
            usage.OrganizationId == organizationId &&
            usage.Date >= start &&
            usage.Date <= end);

        if (request.ApplicationId.HasValue)
        {
            usageQuery = usageQuery.Where(usage =>
                usage.ApplicationId == request.ApplicationId.Value);
        }

        List<ApiConsumption> consumption = await usageQuery
            .Select(usage => new ApiConsumption(
                usage.ApiId,
                usage.Endpoint,
                usage.Date,
                usage.RequestCount,
                usage.ErrorCount))
            .ToListAsync();

        List<OrganizationApiPricing> prices = await db.OrganizationApiPricing
            .Where(pricing => pricing.OrganizationId == organizationId)
            .ToListAsync();

        Dictionary<Guid, string> apiNames = await db.Apis
            .ToDictionaryAsync(api => api.Id, api => api.Name);
        var priceResolver = new UsagePriceResolver(prices);
        var items = consumption
            .GroupBy(item => new
            {
                item.ApiId,
                item.Endpoint,
                Price = priceResolver.FindAt(item.ApiId, item.Date)
            })
            .Where(group => group.Key.Price is not null)
            .Select(group => CreateBillingItem(
                group.Key.ApiId,
                apiNames.GetValueOrDefault(group.Key.ApiId, "API"),
                group.Key.Endpoint,
                group,
                group.Key.Price!))
            .ToList();

        GetBillingResponse response = new(start, end, items.Sum(item => item.Amount), items);
        return Results.Ok(response);
    }

    private static GetBillingItemResponse CreateBillingItem(
        Guid apiId,
        string apiName,
        string endpoint,
        IEnumerable<ApiConsumption> consumption,
        OrganizationApiPricing price)
    {
        var values = consumption.ToList();
        int requests = values.Sum(item => item.Requests);
        int errors = values.Sum(item => item.Errors);
        int billableRequests = values.Sum(item => Math.Max(0, item.Requests - item.Errors));
        decimal amount = billableRequests * price.PricePerRequest;
        decimal averagePrice = billableRequests == 0 ? 0 : amount / billableRequests;

        return new GetBillingItemResponse(
            apiId,
            apiName,
            endpoint,
            requests,
            errors,
            billableRequests,
            averagePrice,
            price.EffectiveFrom,
            amount);
    }
}
