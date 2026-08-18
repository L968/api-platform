namespace ApiPlatform.PortalApi.Features.Usage.GetBilling;

public sealed record GetBillingItemResponse(
    Guid ApiId,
    string Api,
    int Requests,
    decimal PricePerRequest,
    decimal Amount);
