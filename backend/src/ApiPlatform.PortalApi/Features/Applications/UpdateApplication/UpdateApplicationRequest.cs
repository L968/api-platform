using ApiPlatform.PortalApi.Domain.Entities;

namespace ApiPlatform.PortalApi.Features.Applications.UpdateApplication;

public sealed record UpdateApplicationRequest(string Name, ApplicationType Type);
