using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
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

        List<Credential> credentials = await db.Credentials
            .Where(credential => credential.OrganizationId == organizationId)
            .OrderByDescending(credential => credential.CreatedAt)
            .Include(credential => credential.CredentialScopes)
            .ToListAsync();

        Guid[] scopeIds = credentials
            .SelectMany(credential => credential.CredentialScopes)
            .Select(credentialScope => credentialScope.ScopeId)
            .Distinct()
            .ToArray();
        Dictionary<Guid, string> scopeNames = await db.Scopes
            .Where(scope => scopeIds.Contains(scope.Id))
            .ToDictionaryAsync(scope => scope.Id, scope => scope.Name);

        var responses = credentials
            .Select(credential => new ListCredentialsResponse(
                credential.Id,
                credential.Name,
                credential.ClientId,
                credential.ApplicationId,
                credential.CreatedAt,
                credential.ExpiresAt,
                credential.RevokedAt,
                credential.CredentialScopes
                    .Select(credentialScope => scopeNames.GetValueOrDefault(credentialScope.ScopeId, "Unknown"))
                    .Order()
                    .ToArray(),
                credential.IsActive))
            .ToList();

        return Results.Ok(responses);
    }
}
