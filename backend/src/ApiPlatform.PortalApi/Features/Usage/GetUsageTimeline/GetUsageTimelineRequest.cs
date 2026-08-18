namespace ApiPlatform.PortalApi.Features.Usage.GetUsageTimeline;

public sealed record GetUsageTimelineRequest(
    Guid? ApplicationId,
    DateOnly? From,
    DateOnly? To,
    string? Granularity);
