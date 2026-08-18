namespace ApiPlatform.PortalApi.Features.Credentials.CreateCredential;

public sealed record CreateCredentialResponse(
    Guid Id,
    string Name,
    string ClientId,
    string Secret,
    DateTime? ExpiresAt,
    IReadOnlyCollection<Guid> Scopes);
