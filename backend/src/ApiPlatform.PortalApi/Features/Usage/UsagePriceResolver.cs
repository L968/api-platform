using ApiPlatform.PortalApi.Domain.Entities;

namespace ApiPlatform.PortalApi.Features.Usage;

internal sealed class UsagePriceResolver
{
    private readonly Dictionary<Guid, List<OrganizationApiPricing>> _pricesByApi;

    public UsagePriceResolver(IEnumerable<OrganizationApiPricing> prices)
    {
        _pricesByApi = prices
            .GroupBy(price => price.ApiId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(price => price.EffectiveFrom).ToList());
    }

    public decimal PriceAt(Guid apiId, DateOnly date)
    {
        OrganizationApiPricing? price = FindAt(apiId, date);
        return price?.PricePerRequest ?? 0;
    }

    public OrganizationApiPricing? FindAt(Guid apiId, DateOnly date)
    {
        if (!_pricesByApi.TryGetValue(apiId, out List<OrganizationApiPricing>? prices))
        {
            return null;
        }

        return prices.FirstOrDefault(item => item.EffectiveFrom <= date);
    }
}
