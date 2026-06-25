using ApiPlatform.PortalApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Configuration;

internal class OrganizationApiPricingConfiguration : IEntityTypeConfiguration<OrganizationApiPricing>
{
    public void Configure(EntityTypeBuilder<OrganizationApiPricing> builder)
    {
        builder.ToTable("organization_api_pricing");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PricePerRequest)
            .IsRequired()
            .HasColumnType("numeric(10,4)");

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.HasIndex(p => new { p.OrganizationId, p.ApiId })
            .IsUnique();
    }
}
