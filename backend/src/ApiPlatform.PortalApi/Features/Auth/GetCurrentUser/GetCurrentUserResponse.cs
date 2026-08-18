using ApiPlatform.PortalApi.Domain.Entities;

namespace ApiPlatform.PortalApi.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserResponse(
    Guid Id,
    string Email,
    PortalUserStatus Status,
    GetCurrentUserOrganizationResponse Organization);
