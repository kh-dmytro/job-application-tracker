using ApplicationTracker.Api;

namespace ApplicationTracker.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.DisableIPv6", true);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureAppConfiguration(ConfigureAppConfiguration)
                        .ConfigureKestrel((context,serverOptions) =>
                        {
                            if(context.HostingEnvironment.IsDevelopment())
                                serverOptions.Limits.MaxRequestBodySize = 300_000_000;
                        })
                        .UseStartup<Startup>();
                });
        }

        private static void ConfigureAppConfiguration(WebHostBuilderContext builderContext,
            IConfigurationBuilder configBuilder)
        {
            var env = builderContext.HostingEnvironment;
            configBuilder.SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();
        }
    }
}