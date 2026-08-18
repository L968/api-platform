namespace ApiPlatform.PortalApi.Features.Credentials.CreateCredential;

public sealed record CreateCredentialRequest(
    string Name,
    DateTime? ExpiresAt,
    IReadOnlyCollection<Guid> ScopeIds);
