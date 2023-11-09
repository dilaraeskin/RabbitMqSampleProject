using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsumerApp
{
    public class DbContextConfigurator
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DestinationDataContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DestinationConnectionString")));
        }
    }
}
