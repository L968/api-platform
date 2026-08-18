using ApiPlatform.PortalApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Configuration;

internal class CredentialScopeConfiguration : IEntityTypeConfiguration<CredentialScope>
{
    public void Configure(EntityTypeBuilder<CredentialScope> builder)
    {
        builder.ToTable("credential_scope");

        builder.HasKey(cs => new { cs.CredentialId, cs.ScopeId });

        builder.Property(cs => cs.CredentialId)
            .HasColumnName("credential_id");

        builder.Property(cs => cs.ScopeId)
            .HasColumnName("scope_id");

        builder.HasOne<Credential>()
            .WithMany(c => c.CredentialScopes)
            .HasForeignKey(cs => cs.CredentialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Scope>()
            .WithMany()
            .HasForeignKey(cs => cs.ScopeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
