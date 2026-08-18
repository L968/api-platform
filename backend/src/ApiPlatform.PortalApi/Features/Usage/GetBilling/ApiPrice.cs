namespace ApiPlatform.PortalApi.Features.Usage.GetBilling;

internal sealed record ApiPrice(
    Guid ApiId,
    string ApiName,
    decimal PricePerRequest);
