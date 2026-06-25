using ApiPlatform.PortalApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Configuration;

internal class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("application");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        builder.HasOne(a => a.Organization)
            .WithMany()
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.OrganizationId);
    }
}
