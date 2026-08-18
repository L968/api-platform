namespace ApiPlatform.PortalApi.Domain.Entities;

public enum OrganizationStatus
{
    Active,
    Suspended,
    Disabled
}

public sealed class Organization
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public OrganizationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Organization() { }

    public Organization(string name)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        Status = OrganizationStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = OrganizationStatus.Suspended;
    }

    public void Disable()
    {
        Status = OrganizationStatus.Disabled;
    }

    public void Reactivate()
    {
        Status = OrganizationStatus.Active;
    }

    public void Rename(string name)
    {
        Name = name;
    }
}
