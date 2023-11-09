using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Publisher
{
    public class DbContextConfigurator
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SourceDataContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SourceConnectionString")));
        }
    }
}
