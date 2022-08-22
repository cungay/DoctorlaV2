using Doctorla.Host;
using Doctorla.Host.Configurations;
using Doctorla.Infrastructure.Common;
using FluentValidation.AspNetCore;
using Serilog;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddConfigurations();
    builder.Host.UseSerilog((_, config) =>
    {
        config.WriteTo.Console()
        .ReadFrom.Configuration(builder.Configuration);
    });

    builder.Services.AddControllers().AddFluentValidation();
    //builder.Services.AddInfrastructure(builder.Configuration);
    //builder.Services.AddApplication();

    var app = builder.Build();

    //await app.Services.InitializeDatabasesAsync();

    //app.UseInfrastructure(builder.Configuration);
    //app.MapEndpoints();
    app.Run();

}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}
