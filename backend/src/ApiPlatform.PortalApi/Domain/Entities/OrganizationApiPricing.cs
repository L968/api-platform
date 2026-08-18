namespace ApiPlatform.PortalApi.Domain.Entities;

public sealed class OrganizationApiPricing
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApiId { get; private set; }
    public decimal PricePerRequest { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private OrganizationApiPricing() { }

    public OrganizationApiPricing(
        Guid organizationId,
        Guid apiId,
        decimal pricePerRequest,
        DateOnly effectiveFrom)
    {
        Id = Guid.CreateVersion7();
        OrganizationId = organizationId;
        ApiId = apiId;
        PricePerRequest = pricePerRequest;
        EffectiveFrom = effectiveFrom;
        CreatedAt = DateTime.UtcNow;
    }
}
