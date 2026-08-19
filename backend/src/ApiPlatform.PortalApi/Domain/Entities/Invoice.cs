namespace ApiPlatform.PortalApi.Domain.Entities;

public enum InvoiceStatus
{
    Open,
    Paid
}

public sealed class Invoice
{
    private Invoice() { }

    public Invoice(
        Guid organizationId,
        string number,
        DateOnly periodStart,
        DateOnly periodEnd,
        decimal totalAmount,
        DateTime issuedAt,
        DateTime dueAt)
    {
        Id = Guid.CreateVersion7();
        OrganizationId = organizationId;
        Number = number;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        Status = InvoiceStatus.Open;
        TotalAmount = totalAmount;
        IssuedAt = issuedAt;
        DueAt = dueAt;
    }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Number { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime DueAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public List<InvoiceLine> Lines { get; private set; } = [];

    public void MarkPaid(DateTime paidAt)
    {
        if (Status == InvoiceStatus.Paid)
        {
            return;
        }

        Status = InvoiceStatus.Paid;
        PaidAt = paidAt;
    }
}
