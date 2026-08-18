namespace ApiPlatform.PortalApi.Features.Credentials.CreateCredential;

public sealed record CreateCredentialResponse(
    Guid Id,
    string Name,
    string ClientId,
    string ApiKey,
    DateTime? ExpiresAt,
    IReadOnlyCollection<Guid> Scopes);
