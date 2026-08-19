using ApiPlatform.PortalApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Configuration;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoice");
        builder.HasKey(invoice => invoice.Id);
        builder.Property(invoice => invoice.Id).HasColumnName("id");
        builder.Property(invoice => invoice.OrganizationId).HasColumnName("organization_id");
        builder.Property(invoice => invoice.Number).HasColumnName("number").HasMaxLength(50).IsRequired();
        builder.Property(invoice => invoice.PeriodStart).HasColumnName("period_start").HasColumnType("date").IsRequired();
        builder.Property(invoice => invoice.PeriodEnd).HasColumnName("period_end").HasColumnType("date").IsRequired();
        builder.Property(invoice => invoice.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(invoice => invoice.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(12,4)").IsRequired();
        builder.Property(invoice => invoice.IssuedAt).HasColumnName("issued_at").IsRequired();
        builder.Property(invoice => invoice.DueAt).HasColumnName("due_at").IsRequired();
        builder.Property(invoice => invoice.PaidAt).HasColumnName("paid_at");
        builder.HasIndex(invoice => new { invoice.OrganizationId, invoice.Number }).IsUnique();
        builder.HasIndex(invoice => new { invoice.OrganizationId, invoice.PeriodStart, invoice.PeriodEnd }).IsUnique();
        builder.HasMany(invoice => invoice.Lines).WithOne().HasForeignKey(line => line.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}
