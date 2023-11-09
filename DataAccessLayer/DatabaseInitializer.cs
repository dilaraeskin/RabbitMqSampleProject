using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer
{
    public class DatabaseInitializer
    {
        public static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var sourceDataContext = services.GetRequiredService<SourceDataContext>();
                    var destinationDataContext = services.GetRequiredService<DestinationDataContext>();
                    sourceDataContext.Database.EnsureCreated();
                    destinationDataContext.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<DatabaseInitializer>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }
    }
}
