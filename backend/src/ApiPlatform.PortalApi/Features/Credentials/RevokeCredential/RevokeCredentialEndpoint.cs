using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Credentials.RevokeCredential;

public static class RevokeCredentialEndpoint
{
    public static void MapRevokeCredentialEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/credentials/{id:guid}/revoke", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, Guid id, PortalDbContext db)
    {
        Credential? credential = await db.Credentials.SingleOrDefaultAsync(c => c.Id == id && c.OrganizationId == EndpointHelpers.OrganizationId(principal));

        if (credential is null)
        {
            return Results.NotFound();
        }

        credential.Revoke();
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
