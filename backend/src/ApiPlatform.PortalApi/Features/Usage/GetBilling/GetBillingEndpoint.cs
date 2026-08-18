using System.Security.Claims;
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

        List<ApiConsumption> consumption = await db.ApiUsageDaily
            .Where(usage =>
                usage.OrganizationId == organizationId &&
                usage.Date >= start &&
                usage.Date <= end)
            .GroupBy(usage => usage.ApiId)
            .Select(group => new ApiConsumption(
                group.Key,
                group.Sum(usage => usage.RequestCount)))
            .ToListAsync();

        List<ApiPrice> prices = await db.OrganizationApiPricing
            .Where(pricing => pricing.OrganizationId == organizationId)
            .Join(
                db.Apis,
                pricing => pricing.ApiId,
                api => api.Id,
                (pricing, api) => new ApiPrice(api.Id, api.Name, pricing.PricePerRequest))
            .ToListAsync();

        var pricesByApi = prices.ToDictionary(price => price.ApiId);
        var items = consumption
            .Where(item => pricesByApi.ContainsKey(item.ApiId))
            .Select(item =>
                CreateBillingItem(item, pricesByApi[item.ApiId]))
            .ToList();

        GetBillingResponse response = new(start, end, items.Sum(item => item.Amount), items);
        return Results.Ok(response);
    }

    private static GetBillingItemResponse CreateBillingItem(ApiConsumption consumption, ApiPrice price) =>
        new(
            consumption.ApiId,
            price.ApiName,
            consumption.Requests,
            price.PricePerRequest,
            consumption.Requests * price.PricePerRequest);
}
