using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ConsumerApp
{
    public class HostBuilderCreator
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var dbContextConfigurator = new DbContextConfigurator();
                    dbContextConfigurator.ConfigureServices(services, hostContext.Configuration);
                });
        }
    }
}
