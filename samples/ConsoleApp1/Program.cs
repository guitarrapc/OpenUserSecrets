using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args) =>
            await new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    // setup environment
                    var env = hostContext.HostingEnvironment;
                    env.ApplicationName = nameof(ConsoleApp1);
                    env.EnvironmentName = System.Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "production";

                    // json
                    config.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                        .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    // user secrets for local development
                    if (env.IsDevelopment())
                    {
                        // use https://marketplace.visualstudio.com/items?itemName=guitarrapc.OpenUserSecrets to easily manage UserSecrets with GenericHost.
                        config.AddUserSecrets(Assembly.GetExecutingAssembly());
                    }

                    // env
                    config.AddEnvironmentVariables();

                    // args
                    config.AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    logging.SetMinimumLevel(LogLevel.Debug);

                    // Console logger
                    logging.AddConsole();

                    // add other logs you want
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // options
                    services.AddOptions();
                    services.Configure<ConnectionStrings>(hostContext.Configuration.GetSection("ConnectionStrings"));

                    // hosted service = entrypoint
                    services.AddHostedService<TimerHostedService>();

                    // add dependency injection
                    services.AddSingleton<IFugaService, FugaService>();
                })
                .RunConsoleAsync();
    }
}
