namespace ApiPlatform.PortalApi.Features.Organizations.GetOrganization;

public sealed record GetOrganizationRateResponse(
    Guid ApiId,
    string Api,
    decimal PricePerRequest,
    DateOnly? EffectiveFrom,
    decimal? NextPricePerRequest,
    DateOnly? NextEffectiveFrom);
