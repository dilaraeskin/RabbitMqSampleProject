using DataAccessLayer;
using MessageBrokers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PublisherApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                          .ConfigureAppConfiguration(builder =>
                          {
                              builder.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), false);
                          })
                          .ConfigureServices(ConfigureServices)
                          .Build();

            DatabaseInitializer.CreateDbIfNotExists(host, typeof(SourceDataContext));

            host.Run();


            static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
            {
                services.AddRabbitMQServices(context.Configuration);

                services.AddDbContext<SourceDataContext>(options => options.UseSqlServer(context.Configuration.GetConnectionString("SourceConnectionString")));

                services.AddHostedService<PublisherHostedService>();
            }
        }
    }
}
