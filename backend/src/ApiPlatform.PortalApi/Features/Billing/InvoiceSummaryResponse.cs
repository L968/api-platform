namespace ApiPlatform.PortalApi.Features.Billing;

public sealed record InvoiceSummaryResponse(
    Guid Id,
    string Number,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string Status,
    decimal TotalAmount,
    DateTime DueAt,
    DateTime? PaidAt);
