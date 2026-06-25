using ApiPlatform.PortalApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Infrastructure.Persistence;

public sealed class PortalDbContext : DbContext
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<PortalUser> PortalUsers => Set<PortalUser>();
    public DbSet<Api> Apis => Set<Api>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<Scope> Scopes => Set<Scope>();
    public DbSet<CredentialScope> CredentialScopes => Set<CredentialScope>();
    public DbSet<ApiUsageDaily> ApiUsageDaily => Set<ApiUsageDaily>();
    public DbSet<OrganizationApiPricing> OrganizationApiPricing => Set<OrganizationApiPricing>();

    public PortalDbContext(DbContextOptions<PortalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortalDbContext).Assembly);
    }
}
