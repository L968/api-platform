namespace ApiPlatform.PortalApi.Features.Usage.GetBilling;

public sealed record GetBillingRequest(
    DateOnly? From,
    DateOnly? To);
