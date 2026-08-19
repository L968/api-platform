namespace ApiPlatform.PortalApi.Features.Billing;

public sealed record InvoiceDetailResponse(
    Guid Id,
    string Number,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string Status,
    decimal TotalAmount,
    DateTime IssuedAt,
    DateTime DueAt,
    DateTime? PaidAt,
    IReadOnlyCollection<InvoiceLineResponse> Lines);
