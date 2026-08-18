using ApiPlatform.PortalApi.Domain.Entities;

namespace ApiPlatform.PortalApi.Features.Applications.GetApplication;

public sealed record GetApplicationResponse(
    Guid Id,
    string Name,
    ApplicationType Type,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
