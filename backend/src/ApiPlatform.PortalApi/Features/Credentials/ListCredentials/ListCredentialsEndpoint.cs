using System.Security.Claims;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Credentials.ListCredentials;

public static class ListCredentialsEndpoint
{
    public static void MapListCredentialsEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/credentials", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, PortalDbContext db)
    {
        Guid organizationId = EndpointHelpers.OrganizationId(principal);

        List<ListCredentialsResponse> credentials = await db.Credentials
            .Where(credential => credential.OrganizationId == organizationId)
            .OrderByDescending(credential => credential.CreatedAt)
            .Select(credential => new ListCredentialsResponse(
                credential.Id,
                credential.Name,
                credential.ClientId,
                credential.ApplicationId,
                credential.CreatedAt,
                credential.ExpiresAt,
                credential.RevokedAt,
                credential.RevokedAt == null &&
                (credential.ExpiresAt == null || credential.ExpiresAt > DateTime.UtcNow)))
            .ToListAsync();

        return Results.Ok(credentials);
    }
}
