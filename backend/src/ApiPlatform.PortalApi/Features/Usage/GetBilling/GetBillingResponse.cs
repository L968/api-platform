namespace ApiPlatform.PortalApi.Features.Usage.GetBilling;

public sealed record GetBillingResponse(
    DateOnly From,
    DateOnly To,
    decimal Total,
    IReadOnlyCollection<GetBillingItemResponse> Items);
