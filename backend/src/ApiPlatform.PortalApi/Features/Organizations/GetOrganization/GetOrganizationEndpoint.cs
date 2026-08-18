using System.Security.Claims;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Organizations.GetOrganization;

public static class GetOrganizationEndpoint
{
    public static void MapGetOrganizationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/organization", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, PortalDbContext db)
    {
        Guid id = EndpointHelpers.OrganizationId(principal);

        var organization = await db.Organizations
            .Where(organization => organization.Id == id)
            .Select(organization => new
            {
                organization.Id,
                organization.Name,
                organization.Status,
                organization.CreatedAt
            })
            .SingleOrDefaultAsync();

        if (organization is null)
        {
            return Results.NotFound();
        }

        var prices = await db.OrganizationApiPricing
            .Where(price => price.OrganizationId == id)
            .Join(
                db.Apis,
                price => price.ApiId,
                api => api.Id,
                (price, api) => new
                {
                    price.ApiId,
                    Api = api.Name,
                    price.PricePerRequest,
                    price.EffectiveFrom
                })
            .OrderBy(price => price.EffectiveFrom)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var initialPricingDate = new DateOnly(2000, 1, 1);
        var rates = new List<GetOrganizationRateResponse>();
        foreach (var group in prices.GroupBy(price => new { price.ApiId, price.Api }))
        {
            var current = group.LastOrDefault(price => price.EffectiveFrom <= today);
            if (current is null)
            {
                continue;
            }

            var next = group.FirstOrDefault(price => price.EffectiveFrom > today);
            rates.Add(new GetOrganizationRateResponse(
                group.Key.ApiId,
                group.Key.Api,
                current.PricePerRequest,
                current.EffectiveFrom == initialPricingDate ? null : current.EffectiveFrom,
                next?.PricePerRequest,
                next?.EffectiveFrom));
        }

        GetOrganizationResponse response = new(
            organization.Id,
            organization.Name,
            organization.Status,
            organization.CreatedAt,
            rates);

        return Results.Ok(response);
    }
}
