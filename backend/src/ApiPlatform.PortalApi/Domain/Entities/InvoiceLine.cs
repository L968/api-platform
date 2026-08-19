namespace ApiPlatform.PortalApi.Domain.Entities;

public sealed class InvoiceLine
{
    private InvoiceLine() { }

    public InvoiceLine(
        Guid invoiceId,
        Guid apiId,
        string api,
        string endpoint,
        int requests,
        int errors,
        int billableRequests,
        decimal pricePerRequest,
        DateOnly priceEffectiveFrom,
        decimal amount)
    {
        Id = Guid.CreateVersion7();
        InvoiceId = invoiceId;
        ApiId = apiId;
        Api = api;
        Endpoint = endpoint;
        Requests = requests;
        Errors = errors;
        BillableRequests = billableRequests;
        PricePerRequest = pricePerRequest;
        PriceEffectiveFrom = priceEffectiveFrom;
        Amount = amount;
    }

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid ApiId { get; private set; }
    public string Api { get; private set; }
    public string Endpoint { get; private set; }
    public int Requests { get; private set; }
    public int Errors { get; private set; }
    public int BillableRequests { get; private set; }
    public decimal PricePerRequest { get; private set; }
    public DateOnly PriceEffectiveFrom { get; private set; }
    public decimal Amount { get; private set; }
}
