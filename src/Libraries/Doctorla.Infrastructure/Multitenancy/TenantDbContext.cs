using Finbuckle.MultiTenant.Stores;
using Doctorla.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Doctorla.Infrastructure.Multitenancy;

public class TenantDbContext : EFCoreStoreDbContext<DocTenantInfo>
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DocTenantInfo>().ToTable("Tenants", SchemaNames.MultiTenancy);
    }
}