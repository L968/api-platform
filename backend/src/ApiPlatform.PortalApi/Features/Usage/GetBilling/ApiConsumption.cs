namespace ApiPlatform.PortalApi.Features.Usage.GetBilling;

internal sealed record ApiConsumption(
    Guid ApiId,
    string Endpoint,
    DateOnly Date,
    int Requests,
    int Errors);
