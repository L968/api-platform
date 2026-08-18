namespace ApiPlatform.PortalApi.Features.Usage.GetUsage;

public sealed record GetUsageResponse(
    Guid ApiId,
    Guid ApplicationId,
    string Endpoint,
    int Requests,
    int Errors,
    double AverageLatencyMs);
