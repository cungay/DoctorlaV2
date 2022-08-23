using Finbuckle.MultiTenant;
using Doctorla.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Doctorla.Infrastructure.Persistence.Initialization;

internal class ApplicationDbInitializer
{
    private readonly ApplicationDbContext dbContext = null;
    private readonly ITenantInfo currentTenant = null;
    private readonly ApplicationDbSeeder dbSeeder = null;
    private readonly ILogger<ApplicationDbInitializer> logger = null;

    public ApplicationDbInitializer(ApplicationDbContext dbContext, ITenantInfo currentTenant, ApplicationDbSeeder dbSeeder, ILogger<ApplicationDbInitializer> logger)
    {
        this.dbContext = dbContext;
        this.currentTenant = currentTenant;
        this.dbSeeder = dbSeeder;
        this.logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (dbContext.Database.GetMigrations().Any())
        {
            if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                logger.LogInformation("Applying Migrations for '{tenantId}' tenant.", currentTenant.Id);
                await dbContext.Database.MigrateAsync(cancellationToken);
            }

            if (await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                logger.LogInformation("Connection to {tenantId}'s Database Succeeded.", currentTenant.Id);
                await dbSeeder.SeedDatabaseAsync(dbContext, cancellationToken);
            }
        }
    }
}
