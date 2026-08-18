namespace ApiPlatform.PortalApi.Features.Auth.Login;

public sealed record LoginResponse(
    Guid UserId,
    string Email,
    Guid OrganizationId,
    string OrganizationName);
