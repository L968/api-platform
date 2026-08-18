using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Applications.DisableApplication;

public static class DisableApplicationEndpoint
{
    public static void MapDisableApplicationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/applications/{id:guid}/disable", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, Guid id, PortalDbContext db)
    {
        Application? application = await db.Applications.SingleOrDefaultAsync(a => a.Id == id && a.OrganizationId == EndpointHelpers.OrganizationId(principal));
        if (application is null)
        {
            return Results.NotFound();
        }

        application.Disable();
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
