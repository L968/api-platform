using System.Security.Claims;
using System.Security.Cryptography;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using ApiPlatform.PortalApi.Security;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Credentials.CreateCredential;

public static class CreateCredentialEndpoint
{
    public static void MapCreateCredentialEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/applications/{applicationId:guid}/credentials", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, Guid applicationId, CreateCredentialRequest request, PortalDbContext db, PasswordHasher hasher)
    {
        Guid organizationId = EndpointHelpers.OrganizationId(principal);
        Application? application = await db.Applications.SingleOrDefaultAsync(a => a.Id == applicationId && a.OrganizationId == organizationId && a.IsActive);

        if (application is null || string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest();
        }

        var requestedScopes = (request.ScopeIds ?? []).Distinct().ToList();
        List<Guid> validScopes = await db.Scopes
            .Where(scope => requestedScopes.Contains(scope.Id))
            .Select(scope =>
                scope.Id)
            .ToListAsync();

        if (validScopes.Count != requestedScopes.Count)
        {
            return Results.BadRequest("Scope inválido.");
        }

        string clientId = "app_" + Token(9);
        string secret = "sk_" + Token(32);
        Credential credential = new(organizationId, applicationId, request.Name.Trim(), clientId, hasher.Hash(secret), request.ExpiresAt);

        foreach (Guid scopeId in validScopes)
        {
            credential.GrantScope(scopeId);
        }

        db.Credentials.Add(credential);
        await db.SaveChangesAsync();

        CreateCredentialResponse response = new(
            credential.Id,
            credential.Name,
            credential.ClientId,
            $"{credential.ClientId}.{secret}",
            credential.ExpiresAt,
            validScopes);

        return Results.Created($"/credentials/{credential.Id}", response);
    }

    private static string Token(int bytes) => Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes)).Replace("+", "-").Replace("/", "_").TrimEnd('=');
}
