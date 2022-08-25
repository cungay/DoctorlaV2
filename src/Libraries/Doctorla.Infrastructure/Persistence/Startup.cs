using Doctorla.Application.Persistence;
using Doctorla.Domain.Common.Contracts;
using Doctorla.Infrastructure.Common;
using Doctorla.Infrastructure.Persistence.ConnectionString;
using Doctorla.Infrastructure.Persistence.Context;
using Doctorla.Infrastructure.Persistence.Initialization;
using Doctorla.Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Serilog;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Doctorla.Infrastructure.Persistence;

internal static class Startup
{
    private static readonly ILogger logger = Log.ForContext(typeof(Startup));

    internal static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<DatabaseSettings>()
            .BindConfiguration(nameof(DatabaseSettings))
            .PostConfigure(databaseSettings =>
            {
                logger.Information("Current DB Provider: {dbProvider}", databaseSettings.DBProvider);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddSingleton<IDbConnectionFactory>((p) => { return p.UseOrmLite(); })
            .AddSingleton<ICrudEvents>(c => new OrmLiteCrudEvents(c.Resolve<IDbConnectionFactory>()))
            .AddDbContext<ApplicationDbContext>((p, m) => {
                var databaseSettings = p.GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseSettings>>().Value;
                m.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString);
            })
            .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
            .AddTransient<ApplicationDbInitializer>()
            .AddTransient<ApplicationDbSeeder>()
            .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)
            .AddTransient<CustomSeederRunner>()
            .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
            .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()
            .AddRepositories();
    }


    internal static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider, string connectionString)
    {
        return dbProvider.ToLowerInvariant() switch
        {
            DbProviderKeys.Npgsql => builder.UseNpgsql(connectionString, e => e.MigrationsAssembly("Migrators.PostgreSQL")),
            DbProviderKeys.SqlServer => builder.UseSqlServer(connectionString, e => e.MigrationsAssembly("Migrators.MSSQL")),
            DbProviderKeys.MySql => builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
            e => e.MigrationsAssembly("Migrators.MySQL").SchemaBehavior(MySqlSchemaBehavior.Ignore)),
            DbProviderKeys.Oracle => builder.UseOracle(connectionString, e => e.MigrationsAssembly("Migrators.Oracle")),
            DbProviderKeys.SqLite => builder.UseSqlite(connectionString, e => e.MigrationsAssembly("Migrators.SqLite")),
            _ => throw new InvalidOperationException($"DB Provider {dbProvider} is not supported."),
        };
    }

    internal static OrmLiteConnectionFactory UseOrmLite(this IServiceProvider serviceProvider)
    {
        var databaseSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        var dbProvider = databaseSettings.DBProvider;
        var connectionString = databaseSettings.ConnectionString;

        return dbProvider.ToLowerInvariant() switch
        {
            DbProviderKeys.Npgsql => new OrmLiteConnectionFactory(connectionString, PostgreSqlDialect.Provider),
            DbProviderKeys.SqlServer => new OrmLiteConnectionFactory(connectionString, SqlServer2019Dialect.Provider),
            DbProviderKeys.MySql => new OrmLiteConnectionFactory(connectionString, MySqlDialect.Provider),
            /*  
            case DbProviderKeys.Oracle:
                    return new OrmLiteConnectionFactory(connectionString, OracleDialect.Provider);
            */
            DbProviderKeys.SqLite => new OrmLiteConnectionFactory(connectionString, SqliteDialect.Provider),
            _ => throw new InvalidOperationException($"DB Provider {dbProvider} is not supported."),
        };
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Add Repositories
        //services.AddScoped(typeof(IRepository<>), typeof(ApplicationDbRepository<>));

        foreach (var aggregateRootType in
            typeof(IAggregateRoot).Assembly.GetExportedTypes()
                .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass)
                .ToList())
        {
            // Add ReadRepositories.
            services.AddScoped(typeof(IReadRepository<>).MakeGenericType(aggregateRootType), sp =>
                sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)));

            // Decorate the repositories with EntityRepository and expose them as IEntityRepository.
            services.AddScoped(typeof(IEntityRepository<>).MakeGenericType(aggregateRootType), sp =>
                Activator.CreateInstance(
                    typeof(EntityRepository<>).MakeGenericType(aggregateRootType),
                    sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)))
                ?? throw new InvalidOperationException($"Couldn't create EntityRepository for aggregateRootType {aggregateRootType.Name}"));
        }

        return services;
    }
}