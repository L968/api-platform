using ApiPlatform.PortalApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Configuration;

internal class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.ToTable("credential");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(c => c.ApplicationId)
            .HasColumnName("application_id");

        builder.Property(c => c.ClientId)
            .HasColumnName("client_id")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.ClientId)
            .IsUnique();

        builder.Property(c => c.SecretHash)
            .HasColumnName("secret_hash")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(c => c.RevokedAt)
            .HasColumnName("revoked_at");

        builder.HasOne(c => c.Application)
            .WithMany()
            .HasForeignKey(c => c.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.OrganizationId);
        builder.HasIndex(c => c.ApplicationId);

        builder.Navigation(c => c.CredentialScopes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
