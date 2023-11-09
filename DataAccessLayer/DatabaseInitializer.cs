using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer
{
    public class DatabaseInitializer
    {
        public static void CreateDbIfNotExists(IHost host, Type contextType)
        {
            using (var scope = host.Services.CreateScope())
            { 
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService(contextType) as DbContext;
                    context.Database.EnsureCreated();
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
