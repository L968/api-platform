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

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(p => p.ApiId)
            .HasColumnName("api_id");

        builder.Property(p => p.PricePerRequest)
            .HasColumnName("price_per_request")
            .IsRequired()
            .HasColumnType("numeric(10,4)");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(p => new { p.OrganizationId, p.ApiId })
            .IsUnique();
    }
}
