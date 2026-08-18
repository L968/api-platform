using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Applications.DeleteApplication;

public static class DeleteApplicationEndpoint
{
    public static void MapDeleteApplicationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/applications/{id:guid}", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        Guid id,
        PortalDbContext db)
    {
        Guid organizationId = EndpointHelpers.OrganizationId(principal);
        Application? application = await db.Applications.SingleOrDefaultAsync(
            application => application.Id == id && application.OrganizationId == organizationId);

        if (application is null)
        {
            return Results.NotFound();
        }

        List<Credential> credentials = await db.Credentials
            .Where(credential => credential.ApplicationId == id)
            .ToListAsync();

        db.Credentials.RemoveRange(credentials);
        db.Applications.Remove(application);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
