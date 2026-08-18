namespace ApiPlatform.PortalApi.Features.Usage.GetBilling;

public sealed record GetBillingItemResponse(
    Guid ApiId,
    string Api,
    string Endpoint,
    int Requests,
    int Errors,
    int BillableRequests,
    decimal PricePerRequest,
    decimal Amount);
