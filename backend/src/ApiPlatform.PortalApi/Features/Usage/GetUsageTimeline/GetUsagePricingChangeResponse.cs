namespace ApiPlatform.PortalApi.Features.Usage.GetUsageTimeline;

public sealed record GetUsagePricingChangeResponse(
    string Api,
    DateOnly EffectiveFrom,
    decimal? PreviousPricePerRequest,
    decimal PricePerRequest);
