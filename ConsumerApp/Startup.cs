using DataAccessLayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsumerApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var dbContextConfigurator = new DbContextConfigurator();
            dbContextConfigurator.ConfigureServices(services, _configuration);

        }
        public void ConfigureDatabase(IHost host)
        {
            DatabaseInitializer.CreateDbIfNotExists(host);
        }
        public IHostBuilder CreateHostBuilder(string[] args)
        {
            return HostBuilderCreator.CreateHostBuilder(args);
        }
    }
}
