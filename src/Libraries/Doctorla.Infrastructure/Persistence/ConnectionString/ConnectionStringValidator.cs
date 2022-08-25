﻿using Doctorla.Application.Persistence;
using Doctorla.Infrastructure.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;
using System.Data.SqlClient;

namespace Doctorla.Infrastructure.Persistence.ConnectionString;

internal class ConnectionStringValidator : IConnectionStringValidator
{
    private readonly DatabaseSettings dbSettings = null;
    private readonly ILogger<ConnectionStringValidator> logger = null;

    public ConnectionStringValidator(IOptions<DatabaseSettings> dbSettings, ILogger<ConnectionStringValidator> logger)
    {
        this.dbSettings = dbSettings.Value;
        this.logger = logger;
    }

    public bool TryValidate(string connectionString, string? dbProvider = null)
    {
        if (string.IsNullOrWhiteSpace(dbProvider))
        {
            dbProvider = dbSettings.DBProvider;
        }

        try
        {
            switch (dbProvider?.ToLowerInvariant())
            {
                case DbProviderKeys.Npgsql:
                    var postgresqlcs = new NpgsqlConnectionStringBuilder(connectionString);
                    break;

                case DbProviderKeys.MySql:
                    var mysqlcs = new MySqlConnectionStringBuilder(connectionString);
                    break;

                case DbProviderKeys.SqlServer:
                    var mssqlcs = new SqlConnectionStringBuilder(connectionString);
                    break;

                case DbProviderKeys.SqLite:
                    var sqlite = new SqliteConnection(connectionString);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"Connection String Validation Exception : {ex.Message}");
            return false;
        }
    }
}