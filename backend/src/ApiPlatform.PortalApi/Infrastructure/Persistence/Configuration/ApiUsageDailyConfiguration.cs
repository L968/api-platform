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

        builder.Property(u => u.Endpoint)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(u => u.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(u => u.RequestCount)
            .IsRequired();

        builder.Property(u => u.ErrorCount)
            .IsRequired();

        builder.Property(u => u.AvgLatencyMs)
            .IsRequired();

        builder.HasIndex(u => new { u.OrganizationId, u.ApplicationId, u.ApiId, u.Endpoint, u.Date })
            .IsUnique();
    }
}
