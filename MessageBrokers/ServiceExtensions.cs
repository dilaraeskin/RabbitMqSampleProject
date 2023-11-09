using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBrokers
{
    public static class ServiceExtensions
    {
        public static void AddRabbitMQServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMQSettings>(configuration.GetSection(nameof(RabbitMQSettings)));
            services.AddSingleton<RabbitMQService>();
        }
    }
}
