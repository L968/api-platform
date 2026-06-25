namespace ApiPlatform.PortalApi.Domain.Entities;

public sealed class OrganizationApiPricing
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApiId { get; private set; }
    public decimal PricePerRequest { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private OrganizationApiPricing() { }

    public OrganizationApiPricing(Guid organizationId, Guid apiId, decimal pricePerRequest)
    {
        Id = Guid.CreateVersion7();
        OrganizationId = organizationId;
        ApiId = apiId;
        PricePerRequest = pricePerRequest;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        PricePerRequest = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}
