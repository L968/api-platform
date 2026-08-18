using ApiPlatform.PortalApi.Domain.Entities;

namespace ApiPlatform.PortalApi.Features.Organizations.GetOrganization;

public sealed record GetOrganizationResponse(
    Guid Id,
    string Name,
    OrganizationStatus Status,
    DateTime CreatedAt);
