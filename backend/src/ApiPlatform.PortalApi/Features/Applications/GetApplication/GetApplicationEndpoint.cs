using System.Security.Claims;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Applications.GetApplication;

public static class GetApplicationEndpoint
{
    public static void MapGetApplicationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/applications/{id:guid}", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, Guid id, PortalDbContext db)
    {
        GetApplicationResponse? application = await db.Applications
            .Where(application =>
                application.Id == id &&
                application.OrganizationId == EndpointHelpers.OrganizationId(principal))
            .Select(application => new GetApplicationResponse(
                application.Id,
                application.Name,
                application.Type,
                application.IsActive,
                application.CreatedAt,
                application.UpdatedAt))
            .SingleOrDefaultAsync();

        return application is null ? Results.NotFound() : Results.Ok(application);
    }
}
