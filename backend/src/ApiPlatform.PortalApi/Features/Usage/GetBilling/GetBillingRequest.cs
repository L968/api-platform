namespace ApiPlatform.PortalApi.Features.Usage.GetBilling;

public sealed record GetBillingRequest(
    Guid? ApplicationId,
    DateOnly? From,
    DateOnly? To);
