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

        builder.Property(c => c.ClientId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.ClientId)
            .IsUnique();

        builder.Property(c => c.SecretHash)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.ExpiresAt);

        builder.Property(c => c.RevokedAt);

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
