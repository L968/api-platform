namespace ApiPlatform.PortalApi.Features.Credentials.ListCredentials;

public sealed record ListCredentialsResponse(
    Guid Id,
    string Name,
    string ClientId,
    Guid ApplicationId,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? RevokedAt,
    bool IsActive);
