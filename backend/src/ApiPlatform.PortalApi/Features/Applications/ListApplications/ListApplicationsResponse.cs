using ApiPlatform.PortalApi.Domain.Entities;

namespace ApiPlatform.PortalApi.Features.Applications.ListApplications;

public sealed record ListApplicationsResponse(
    Guid Id,
    string Name,
    ApplicationType Type,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
