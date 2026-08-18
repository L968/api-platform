using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Usage.GetUsage;

public static class GetUsageEndpoint
{
    public static void MapGetUsageEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/usage", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, [AsParameters] GetUsageRequest request, PortalDbContext db)
    {
        IQueryable<ApiUsageDaily> query = db.ApiUsageDaily.Where(u => u.OrganizationId == EndpointHelpers.OrganizationId(principal));
        if (request.ApplicationId.HasValue)
        {
            query = query.Where(usage => usage.ApplicationId == request.ApplicationId.Value);
        }

        if (request.From.HasValue)
        {
            query = query.Where(usage => usage.Date >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(usage => usage.Date <= request.To.Value);
        }

        List<GetUsageResponse> items = await query
            .GroupBy(usage => new { usage.ApiId, usage.ApplicationId, usage.Endpoint })
            .Select(group => new GetUsageResponse(
                group.Key.ApiId,
                group.Key.ApplicationId,
                group.Key.Endpoint,
                group.Sum(usage => usage.RequestCount),
                group.Sum(usage => usage.ErrorCount),
                group.Average(usage => usage.AvgLatencyMs)))
            .ToListAsync();

        return Results.Ok(items);
    }
}

