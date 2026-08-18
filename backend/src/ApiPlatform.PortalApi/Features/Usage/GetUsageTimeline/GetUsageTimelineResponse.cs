namespace ApiPlatform.PortalApi.Features.Usage.GetUsageTimeline;

public sealed record GetUsageTimelineResponse(
    DateOnly From,
    DateOnly To,
    string Granularity,
    IReadOnlyCollection<GetUsageTimelinePointResponse> Items);
