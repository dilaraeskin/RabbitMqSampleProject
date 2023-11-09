using DataAccessLayer;
using DataContract;
using MessageBrokers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsumerApp
{
    public class Program
    {
        private static readonly string _queueName = "my_queue";
        private static RabbitMQService _rabbitMQService;
        private static RabbitMQConnectionService _rabbitMQConnectionService;

        public static void Main(string[] args)
        {
            var appsettingsConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("rabbitMQSettings.json", optional: false)
                .Build();

            var startup = new Startup(appsettingsConfiguration);

            var hostBuilder = startup.CreateHostBuilder(args);
            var host = hostBuilder.Build();
            startup.ConfigureDatabase(host);

            var serviceProvider = host.Services;
            var dataContext = serviceProvider.GetRequiredService<DestinationDataContext>();

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var rabbitMQSettings = configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();

            _rabbitMQService = new RabbitMQService(rabbitMQSettings);

            var target = dataContext.Set<Target>();
            var rabbitMQPublisher = new RabbitMQConsumer(_rabbitMQService);
            rabbitMQPublisher.ConsumeDataToTargetTable(_queueName, dataContext, target);

            host.Run();
        }
    }
}
