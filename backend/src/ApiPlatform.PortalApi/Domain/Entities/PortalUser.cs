namespace ApiPlatform.PortalApi.Domain.Entities;

public enum PortalUserStatus
{
    Active,
    Disabled
}

public sealed class PortalUser
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public PortalUserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Organization? Organization { get; private set; }

    private PortalUser() { }

    public PortalUser(Guid organizationId, string email, string passwordHash)
    {
        Id = Guid.CreateVersion7();
        OrganizationId = organizationId;
        Email = email;
        PasswordHash = passwordHash;
        Status = PortalUserStatus.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Status = PortalUserStatus.Disabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Status = PortalUserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}
