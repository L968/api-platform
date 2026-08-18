using ApiPlatform.PortalApi.Domain.Entities;

namespace ApiPlatform.PortalApi.Features.Applications.CreateApplication;

public sealed record CreateApplicationResponse(
    Guid Id,
    string Name,
    ApplicationType Type,
    bool IsActive);
