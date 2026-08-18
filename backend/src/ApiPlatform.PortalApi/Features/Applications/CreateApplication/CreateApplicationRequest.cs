using ApiPlatform.PortalApi.Domain.Entities;

namespace ApiPlatform.PortalApi.Features.Applications.CreateApplication;

public sealed record CreateApplicationRequest(string Name, ApplicationType Type);
