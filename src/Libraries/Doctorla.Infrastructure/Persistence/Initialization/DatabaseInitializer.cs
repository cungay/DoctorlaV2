using Finbuckle.MultiTenant;
using Doctorla.Infrastructure.Multitenancy;
using Doctorla.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Doctorla.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer : IDatabaseInitializer
{
    private readonly TenantDbContext tenantDbContext = null;
    private readonly IServiceProvider serviceProvider = null;
    private readonly ILogger<DatabaseInitializer> logger = null;

    public DatabaseInitializer(TenantDbContext tenantDbContext, IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        this.tenantDbContext = tenantDbContext;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
    {
        await InitializeTenantDbAsync(cancellationToken);

        foreach (var tenant in await tenantDbContext.TenantInfo.ToListAsync(cancellationToken))
        {
            await InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
        }
    }

    public async Task InitializeApplicationDbForTenantAsync(DocTenantInfo tenant, CancellationToken cancellationToken)
    {
        // First create a new scope
        using var scope = serviceProvider.CreateScope();

        // Then set current tenant so the right connectionstring is used
        serviceProvider.GetRequiredService<IMultiTenantContextAccessor>()
            .MultiTenantContext = new MultiTenantContext<DocTenantInfo>()
            {
                TenantInfo = tenant
            };

        // Then run the initialization in the new scope
        await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
            .InitializeAsync(cancellationToken);
    }

    private async Task InitializeTenantDbAsync(CancellationToken cancellationToken)
    {
        if (tenantDbContext.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("Applying Root Migrations.");
            await tenantDbContext.Database.MigrateAsync(cancellationToken);
        }
        await SeedRootTenantAsync(cancellationToken);
    }

    private async Task SeedRootTenantAsync(CancellationToken cancellationToken)
    {
        if (await tenantDbContext.TenantInfo.FindAsync(new object?[] { MultitenancyConstants.Root.Id }, cancellationToken: cancellationToken) is null)
        {
            var rootTenant = new DocTenantInfo(
                MultitenancyConstants.Root.Id,
                MultitenancyConstants.Root.Name,
                string.Empty,
                MultitenancyConstants.Root.EmailAddress);

            rootTenant.SetValidity(DateTime.UtcNow.AddYears(1));

            tenantDbContext.TenantInfo.Add(rootTenant);

            await tenantDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}