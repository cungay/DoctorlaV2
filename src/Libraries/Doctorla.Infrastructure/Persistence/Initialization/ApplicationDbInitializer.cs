using Finbuckle.MultiTenant;
using Doctorla.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Doctorla.Infrastructure.Persistence.Initialization;

internal class ApplicationDbInitializer
{
    private readonly IDbConnectionFactory dbContext = null;
    private readonly ITenantInfo currentTenant = null;
    private readonly ApplicationDbSeeder dbSeeder = null;
    private readonly ILogger<ApplicationDbInitializer> logger = null;

    public ApplicationDbInitializer(IDbConnectionFactory dbContext, ITenantInfo currentTenant, ApplicationDbSeeder dbSeeder, ILogger<ApplicationDbInitializer> logger)
    {
        this.dbContext = dbContext;
        this.currentTenant = currentTenant;
        this.dbSeeder = dbSeeder;
        this.logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var db = dbContext.OpenDbConnection();
        }
        catch (Exception)
        {
            logger.LogInformation("Connection to {tenantId}'s Database Failed.", currentTenant.Id);
            throw;
        }

        logger.LogInformation("Connection to {tenantId}'s Database Succeeded.", currentTenant.Id);
        await dbSeeder.SeedDatabaseAsync(dbContext, cancellationToken);
    }
}
