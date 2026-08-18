using ApiPlatform.PortalApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Configuration;

internal class ApiUsageDailyConfiguration : IEntityTypeConfiguration<ApiUsageDaily>
{
    public void Configure(EntityTypeBuilder<ApiUsageDaily> builder)
    {
        builder.ToTable("api_usage_daily");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(u => u.ApplicationId)
            .HasColumnName("application_id");

        builder.Property(u => u.ApiId)
            .HasColumnName("api_id");

        builder.Property(u => u.Endpoint)
            .HasColumnName("endpoint")
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(u => u.Date)
            .HasColumnName("date")
            .IsRequired()
            .HasColumnType("date");

        builder.Property(u => u.RequestCount)
            .HasColumnName("request_count")
            .IsRequired();

        builder.Property(u => u.ErrorCount)
            .HasColumnName("error_count")
            .IsRequired();

        builder.Property(u => u.AvgLatencyMs)
            .HasColumnName("avg_latency_ms")
            .IsRequired();

        builder.HasIndex(u => new { u.OrganizationId, u.ApplicationId, u.ApiId, u.Endpoint, u.Date })
            .IsUnique();
    }
}
