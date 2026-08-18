using System.Security.Claims;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Applications.ListApplications;

public static class ListApplicationsEndpoint
{
    public static void MapListApplicationsEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/applications", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, PortalDbContext db) =>
        Results.Ok(await db.Applications
            .Where(application => application.OrganizationId == EndpointHelpers.OrganizationId(principal))
            .OrderBy(application => application.Name)
            .Select(application => new ListApplicationsResponse(
                application.Id,
                application.Name,
                application.Type,
                application.IsActive,
                application.CreatedAt,
                application.UpdatedAt))
            .ToListAsync());
}
