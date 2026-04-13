using System.Reflection;
using ApplicationTracker.Common.Data;
using CafeteriaApp.Common.Commands;
using CafeteriaApp.Common.Queries;


namespace ApplicationTracker.Api
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env          = env;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.WithOrigins(
                            "https://cafeteria-app.azurewebsites.net",
                            "http://localhost:3002"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            
            if (_env.IsDevelopment())
            {/*
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "LocalEasyAuth";
                        options.DefaultChallengeScheme    = "LocalEasyAuth";
                    })
                    .AddScheme<AuthenticationSchemeOptions, LocalEasyAuthHandler>("LocalEasyAuth", _ => { });*/
            }
            else
            {
                /*services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = "https://login.microsoftonline.com/common/v2.0";
                        options.TokenValidationParameters = new()
                        {
                            ValidateAudience = true,
                            ValidateIssuer   = true,
                            // ValidIssuers     = new[]
                            // {
                            //     // PV/AWK Tenant
                            //     "https://login.microsoftonline.com/580b7714-e0ba-4b1b-a1f9-315db4623b1f/v2.0",
                            //     // Tenant ID AWK (all users from awk)
                            //     "https://login.microsoftonline.com/15535e10-9847-45dd-942e-244fc1f74522/v2.0",
                            // },
                            ValidIssuers = new[]
                            {
                                // v1 Format
                                "https://sts.windows.net/580b7714-e0ba-4b1b-a1f9-315db4623b1f/",
                                "https://sts.windows.net/15535e10-9847-45dd-942e-244fc1f74522/",
                                // v2 Format
                                "https://login.microsoftonline.com/580b7714-e0ba-4b1b-a1f9-315db4623b1f/v2.0",
                                "https://login.microsoftonline.com/15535e10-9847-45dd-942e-244fc1f74522/v2.0",
                            },
                            ValidAudiences = new[]
                            {
                                "6a2d3ad7-7b97-46cb-ab8f-f15fad91ad7a",// cafeteria-app
                                "faafe262-2c80-41c9-8388-1b1c41368b8a",  // cafeteria-api
                                "api://faafe262-2c80-41c9-8388-1b1c41368b8a"
                            }
                        };
                    });*/
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AppDbContext"),
                    b => {
                        b.UseNetTopologySuite();
                        b.CommandTimeout(120);
                    }));

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = 
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            AddAppConfiguration(services, Configuration);
            ConfigureDependencyInjection(services);
            services.AddHttpContextAccessor();
            // services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        }


        private static void AddAppConfiguration(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabaseConfiguration>(config =>
                config.AppDbContext = configuration.GetConnectionString("AppDbContext"));
        }


        private void ConfigureDependencyInjection(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblies(Assembly.Load("ApplicationTracker.Common"), Assembly.Load("ApplicationTracker.Api"))
                .AddClasses(c => c.AssignableToAny(
                    typeof(ICommandHandler<>), 
                    typeof(ICommandHandler<,>), 
                    typeof(IQueryHandler<,>))
                )
                // .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            
            services.AddScoped<ICommandProcessor, CommandProcessor>();
            services.AddScoped<IQueryProcessor, QueryProcessor>();

            services.AddScoped<AppDbContext>();
            // services.AddScoped<IClaimsTransformation, UserRoleClaimsTransformer>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext dbContext)
        {
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
        }
    }
}