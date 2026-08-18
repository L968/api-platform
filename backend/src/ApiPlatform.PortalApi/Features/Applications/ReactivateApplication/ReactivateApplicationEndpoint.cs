using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Applications.ReactivateApplication;

public static class ReactivateApplicationEndpoint
{
    public static void MapReactivateApplicationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/applications/{id:guid}/reactivate", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, Guid id, PortalDbContext db)
    {
        Application? application = await db.Applications.SingleOrDefaultAsync(a => a.Id == id && a.OrganizationId == EndpointHelpers.OrganizationId(principal));
        if (application is null)
        {
            return Results.NotFound();
        }

        application.Reactivate();
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
