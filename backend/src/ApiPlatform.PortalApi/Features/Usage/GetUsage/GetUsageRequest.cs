namespace ApiPlatform.PortalApi.Features.Usage.GetUsage;

public sealed record GetUsageRequest(
    Guid? ApplicationId,
    DateOnly? From,
    DateOnly? To);
