namespace ApiPlatform.PortalApi.Features.Billing;

public sealed record InvoiceLineResponse(
    Guid Id,
    Guid ApiId,
    string Api,
    string Endpoint,
    int Requests,
    int Errors,
    int BillableRequests,
    decimal PricePerRequest,
    DateOnly PriceEffectiveFrom,
    decimal Amount);
