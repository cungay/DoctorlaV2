using System.Data;
using Doctorla.Domain.Identity;
using Doctorla.Infrastructure.Auth;
using Doctorla.Infrastructure.Persistence;
using Funq;
using ServiceStack;
using ServiceStack.Admin;
using ServiceStack.Api.OpenApi;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.Logging;
using ServiceStack.Messaging;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using static ServiceStack.Inspect;

[assembly: HostingStartup(typeof(Doctorla.Host.AppHost))]

namespace Doctorla.Host;

public class AppHost : AppHostBase, IHostingStartup
{
    #region Ctor

    public AppHost() : base("Doctorla V2!", typeof(Doctorla.Application.Identity.Users.CreateUserRequest).Assembly) { }

    #endregion
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) =>
        {
            const string configurationsDirectory = "Configurations";
            var env = context.HostingEnvironment;
            
            //context.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //     .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            //     .AddJsonFile($"{configurationsDirectory}/database.json", optional: false, reloadOnChange: true)
            //     .AddJsonFile($"{configurationsDirectory}/database.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            //     .AddEnvironmentVariables();

            AppSettings = new NetCoreAppSettings(context.Configuration);

            // Configure ASP.NET Core IOC Dependencies
            //services.AddSingleton<IMessageService>(c => new BackgroundMqService());

            var cs = "Data Source=.;Initial Catalog=DoctorlaV2;Integrated Security=False;Persist Security Info=False;User ID=sa;Password=1";
            var db = AppSettings.Get<DatabaseSettings>("DatabaseSettings");

            var databaseSettings = context.Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();

            //string? rootConnectionString = databaseSettings.ConnectionString;
            //if (string.IsNullOrEmpty(rootConnectionString))
            //{
            //    throw new InvalidOperationException("DB ConnectionString is not configured.");
            //}

            //string? dbProvider = databaseSettings.DBProvider;
            //if (string.IsNullOrEmpty(dbProvider))
            //{
            //    throw new InvalidOperationException("DB Provider is not configured.");
            //}

            var dbFactory = new OrmLiteConnectionFactory(
                cs, SqlServer2019Dialect.Provider);
            dbFactory.RegisterDialectProvider(nameof(SqlServer2019Dialect), SqlServer2019Dialect.Provider);

            services.AddSingleton<IDbConnectionFactory>(dbFactory);
        })
        .Configure(app =>
        {
            // Configure ASP.NET Core App
            if (!HasInit)
                app.UseServiceStack(new AppHost()
                {
                    AppSettings = new MultiAppSettingsBuilder()
                    .AddNetCore(Configuration)
                    .AddAppSettings()
                    .AddEnvironmentalVariables()
                    .AddTextFile("Configurations/database.json".MapProjectPath())
                    .Build()
                });
        })
        .ConfigureAppHost(afterAppHostInit: appHost =>
        {
            /*
            var mqServer = appHost.Resolve<IMessageService>();

            mqServer.RegisterHandler<SendNotification>(appHost.ExecuteMessage, 4);
            mqServer.RegisterHandler<SendSystemEmail>(appHost.ExecuteMessage);
            mqServer.RegisterHandler<SendEmail>(appHost.ExecuteMessage);
            mqServer.Start();

            appHost.ExecuteService(new RetryPendingNotifications());
            */
        });

    private static ILog? log;

    public override void OnStartupException(Exception ex)
    {
        base.OnStartupException(ex);
    }

    // Configure your AppHost with the necessary configuration and dependencies your App needs
    public override void Configure(Container container)
    {
        //LogManager.LogFactory = new ConsoleLogFactory(debugEnabled:true);
        log = LogManager.GetLogger(typeof(AppHost));

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

        var dbFactory = container.Resolve<IDbConnectionFactory>();

        var authRepo = new OrmLiteAuthRepository<CustomUserAuth, UserAuthDetails>(dbFactory);
        container.Register<IUserAuthRepository>(authRepo);
        authRepo.InitSchema();
        using var db = dbFactory.OpenDbConnection();
        var lockedUserIds = db.ColumnDistinct<int>(
            db.From<CustomUserAuth>().Where(x => x.LockedDate != null).Select(x => x.Id));

        /*
        db.CreateTableIfNotExists<Organization>();
        db.CreateTableIfNotExists<OrganizationMember>();
        db.CreateTableIfNotExists<OrganizationLabel>();
        db.CreateTableIfNotExists<OrganizationMemberInvite>();
        db.CreateTableIfNotExists<OrganizationSubscription>();
        db.CreateTableIfNotExists<Category>();
        db.CreateTableIfNotExists<PageStats>();
        db.CreateTableIfNotExists<Post>();
        db.CreateTableIfNotExists<PostVote>();
        db.CreateTableIfNotExists<PostFavorite>();
        db.CreateTableIfNotExists<Notification>();
        db.CreateTableIfNotExists<UserActivity>();
        db.CreateTableIfNotExists<TechnologyStack>();
        db.CreateTableIfNotExists<Technology>();
        db.CreateTableIfNotExists<TechnologyHistory>();
        db.CreateTableIfNotExists<TechnologyChoice>();
        db.CreateTableIfNotExists<TechnologyStackHistory>();
        db.CreateTableIfNotExists<UserFavoriteTechnologyStack>();
        db.CreateTableIfNotExists<UserFavoriteTechnology>();
        */

        Plugins.Add(new AuthFeature(() => new CustomUserSession(), new IAuthProvider[] {
            new TwitterAuthProvider(AppSettings),
            new GithubAuthProvider(AppSettings),
            new JwtAuthProvider(AppSettings) {
                RequireSecureConnection = false,
                UseTokenCookie = true,
                // Ensure locked accounts are enforced prior to their JWT expiring
                ValidateToken = (payload, req) => {
                    if (lockedUserIds.Contains(payload.GetValue("sub", () => "0").ToInt()))
                        throw new AuthenticationException(ErrorMessages.UserAccountLocked.Localize(req));
                    return true;
                },
                CreatePayloadFilter = (payload, session) => {
                    var githubAuth = session.ProviderOAuthAccess.Safe()
                        .FirstOrDefault(x => x.Provider == "github");
                    payload["ats"] = githubAuth?.AccessTokenSecret;
                },
                PopulateSessionFilter = (session, obj, req) => {
                    session.ProviderOAuthAccess = new List<IAuthTokens> {
                        new AuthTokens {Provider = "github", AccessTokenSecret = obj["ats"]}
                    };
                }
            }
            //new DiscourseAuthProvider {
            //    Provider = "servicestack",
            //    DiscourseUrl = "https://forums.servicestack.net",
            //},
        })
        {
            HtmlRedirect = "/"
        });

        /*
        container.Register(new EmailProvider
        {
            UserName = Environment.GetEnvironmentVariable("TECHSTACKS_SMTP_USER") ??
                       AppSettings.GetString("smtp.UserName"),
            Password = Environment.GetEnvironmentVariable("TECHSTACKS_SMTP_PASS") ??
                       AppSettings.GetString("smtp.Password"),
            EnableSsl = true,
            Host = AppSettings.GetString("smtp.Host"),
            Port = AppSettings.Get<int>("smtp.Port"),
            Bcc = AppSettings.GetString("smtp.Bcc"),
        });
        */

        //Plugins.Add(new AdminUsersFeature());

        /*
        Plugins.Add(new CorsFeature(
            allowOriginWhitelist: new[] {
                "https://techstacks.io", "https://www.techstacks.io",
                "http://localhost:3000", "http://localhost:16325", "http://localhost:8080", "http://null.jsbin.com",
                "http://run.plnkr.co"
            },
            allowCredentials: true,
            allowedHeaders: "Content-Type, Allow, Authorization",
            maxAge: 60 * 60)); //Cache OPTIONS permissions
        */

        /*
        Plugins.Add(new ValidationFeature());
        container.RegisterValidators(typeof(AppHost).Assembly);
        container.RegisterValidators(typeof(TechnologyServices).Assembly);
        */

        /*
        Plugins.Add(new AutoQueryMetadataFeature
        {
            AutoQueryViewerConfig = {
                ServiceDescription = "Discover what technologies were used to create popular Websites and Apps",
                ServiceIconUrl = "/img/app/logo-76.png",
                BackgroundColor = "#0095F5",
                TextColor = "#fff",
                LinkColor = "#ffff8d",
                BrandImageUrl = "/img/app/brand.png",
                BrandUrl = "https://techstacks.io",
                BackgroundImageUrl = "/img/app/bg.png",
                IsPublic = true,
                OnlyShowAnnotatedServices = true,
            }
        });
        */

        /*
        Plugins.Add(new AutoQueryFeature
        {
            MaxLimit = 100,
            StripUpperInLike = false,
            IncludeTotal = true,
            ResponseFilters = {
#if DEBUG
                ctx => ctx.Response.Meta["Cache"] = Stopwatch.GetTimestamp().ToString()
#endif
            }
        });
        */

        //Plugins.Add(new AdminFeature());

        Plugins.Add(new PostmanFeature());
        Plugins.Add(new OpenApiFeature());

        //RegisterTypedRequestFilter<IRegisterStats>((req, res, dto) =>
        //    dbFactory.RegisterPageView(dto.GetStatsId()));

        //if (Config.DebugMode)
        //{
        //    Plugins.Add(new LispReplTcpServer
        //    {
        //        ScriptMethods = {
        //            new DbScriptsAsync()
        //        },
        //        ScriptNamespaces = {
        //            nameof(TechStacks),
        //            $"{nameof(TechStacks)}.{nameof(ServiceInterface)}",
        //            $"{nameof(TechStacks)}.{nameof(ServiceModel)}",
        //        },
        //    });
        //}
    }

}
