using System.Reflection;
using ApplicationTracker.Api.Configuration;
using ApplicationTracker.Api.Services;
using ApplicationTracker.Common.Commands;
using ApplicationTracker.Common.Data;
using CafeteriaApp.Common.Commands;
using CafeteriaApp.Common.Queries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;


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
                
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("AppDbContext"),
                    b => {
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
            services.AddScoped<JwtTokenService>();
            services.AddScoped<ApplicationService>();
            services.AddScoped<LinkedinService>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();
            services.AddScoped<XingService>();
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