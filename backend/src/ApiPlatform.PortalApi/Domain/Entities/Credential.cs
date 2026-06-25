namespace ApiPlatform.PortalApi.Domain.Entities;

public sealed class Credential
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string ClientId { get; private set; }
    public string SecretHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public Application? Application { get; private set; }

    private readonly List<CredentialScope> _credentialScopes = [];
    public IReadOnlyCollection<CredentialScope> CredentialScopes => _credentialScopes;

    private Credential() { }

    public Credential(Guid organizationId, Guid applicationId, string clientId, string secretHash, DateTime? expiresAt = null)
    {
        Id = Guid.CreateVersion7();
        OrganizationId = organizationId;
        ApplicationId = applicationId;
        ClientId = clientId;
        SecretHash = secretHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsActive =>
        RevokedAt is null && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }

    public void GrantScope(Guid scopeId)
    {
        if (_credentialScopes.Any(cs => cs.ScopeId == scopeId))
        {
            return;
        }

        _credentialScopes.Add(new CredentialScope(Id, scopeId));
    }
}
