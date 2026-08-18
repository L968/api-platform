using ApiPlatform.PortalApi.Domain.Entities;

namespace ApiPlatform.PortalApi.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserOrganizationResponse(
    Guid Id,
    string Name,
    OrganizationStatus Status);
