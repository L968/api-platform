namespace ApiPlatform.PortalApi.Features.Usage.GetUsageTimeline;

public sealed record GetUsageTimelinePointResponse(
    DateOnly PeriodStart,
    int Requests,
    int Errors,
    double AverageLatencyMs,
    decimal Cost);
