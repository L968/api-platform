namespace ApiPlatform.PortalApi.Domain.Entities;

public sealed class CredentialScope
{
    public Guid CredentialId { get; private set; }
    public Guid ScopeId { get; private set; }

    private CredentialScope() { }

    public CredentialScope(Guid credentialId, Guid scopeId)
    {
        CredentialId = credentialId;
        ScopeId = scopeId;
    }
}
