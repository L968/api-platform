using ApiPlatform.PortalApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Configuration;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_line");
        builder.HasKey(line => line.Id);
        builder.Property(line => line.Id).HasColumnName("id");
        builder.Property(line => line.InvoiceId).HasColumnName("invoice_id");
        builder.Property(line => line.ApiId).HasColumnName("api_id");
        builder.Property(line => line.Api).HasColumnName("api").HasMaxLength(100).IsRequired();
        builder.Property(line => line.Endpoint).HasColumnName("endpoint").HasMaxLength(300).IsRequired();
        builder.Property(line => line.Requests).HasColumnName("requests").IsRequired();
        builder.Property(line => line.Errors).HasColumnName("errors").IsRequired();
        builder.Property(line => line.BillableRequests).HasColumnName("billable_requests").IsRequired();
        builder.Property(line => line.PricePerRequest).HasColumnName("price_per_request").HasColumnType("numeric(10,4)").IsRequired();
        builder.Property(line => line.PriceEffectiveFrom).HasColumnName("price_effective_from").HasColumnType("date").IsRequired();
        builder.Property(line => line.Amount).HasColumnName("amount").HasColumnType("numeric(12,4)").IsRequired();
        builder.HasIndex(line => line.InvoiceId);
    }
}
