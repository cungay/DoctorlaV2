using ServiceStack.Api.OpenApi;
using ServiceStack;
using Funq;
using ServiceStack.Text;
using ServiceStack.Configuration;
using Doctorla.Infrastructure.Persistence;

[assembly: HostingStartup(typeof(Doctorla.Host.AppHost))]

namespace Doctorla.Host;

public class AppHost : AppHostBase, IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) =>
        {
            // Configure ASP.NET Core IOC Dependencies
        });

    public AppHost() : base("MvcAuthApp", typeof(Doctorla.Application.Identity.Users.CreateUserRequest).Assembly) { }

    public override void Configure(Container container)
    {
        var db = AppSettings.Get<DatabaseSettings>("DatabaseSettings");

        var dbFactory = new ServiceStack.OrmLite.OrmLiteConnectionFactory(
            db.ConnectionString, ServiceStack.OrmLite.SqlServer2019Dialect.Provider);
        dbFactory.RegisterDialectProvider(nameof(ServiceStack.OrmLite.SqlServer2019Dialect), ServiceStack.OrmLite.SqlServer2019Dialect.Provider);

        container.AddSingleton<ServiceStack.Data.IDbConnectionFactory>(dbFactory);

        SetConfig(new HostConfig
        {
            // UseSameSiteCookies = true,
            AddRedirectParamsToQueryString = true,
            DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), false),
            DefaultRedirectPath = "/swagger-ui",
            GlobalResponseHeaders = {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
                { "Access-Control-Allow-Headers", "Content-Type" },
              },
        });

        JsConfig.Init(new ServiceStack.Text.Config
        {
            DateHandler = DateHandler.ISO8601
        });

        Plugins.Add(new PostmanFeature());

        Plugins.Add(new OpenApiFeature()
        {
            UseCamelCaseSchemaPropertyNames = true,
            UseLowercaseUnderscoreSchemaPropertyNames = true,
            UseBearerSecurity = true,
            //UseBasicSecurity = true,
            //AnyRouteVerbs = new List<string> { HttpMethods.Get, HttpMethods.Post }
        });
    }
}