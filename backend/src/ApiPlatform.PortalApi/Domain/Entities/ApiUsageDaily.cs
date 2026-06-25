namespace ApiPlatform.PortalApi.Domain.Entities;

/// <summary>
/// Daily consumption aggregate, populated by the background process (Meter).
/// The PortalApi only reads this entity, it never creates or updates it.
/// </summary>
public sealed class ApiUsageDaily
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid ApiId { get; private set; }
    public string Endpoint { get; private set; }
    public DateOnly Date { get; private set; }
    public int RequestCount { get; private set; }
    public int ErrorCount { get; private set; }
    public int AvgLatencyMs { get; private set; }

    private ApiUsageDaily() { }
}
