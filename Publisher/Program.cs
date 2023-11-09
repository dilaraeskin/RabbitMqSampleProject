using DataAccessLayer;
using DataContract;
using MessageBrokers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Publisher
{
    public class Program
    {
        private static readonly string _queueName = "my_queue";
        private static RabbitMQService _rabbitMQService;

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
            var dataContext = serviceProvider.GetRequiredService<SourceDataContext>();

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var rabbitMQSettings = configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();

            _rabbitMQService = new RabbitMQService(rabbitMQSettings);

            var source = dataContext.Set<Source>();

            var rabbitMQPublisher = new RabbitMQPublisher(_rabbitMQService);
            rabbitMQPublisher.PublishDataFromSourceTable(_queueName, dataContext, source);

            host.Run();
        }
    }

    //public class PublisherHostedService : BackgroundService
    //{
    //    private readonly RabbitMQService _rabbitMQService;

    //    public PublisherHostedService(RabbitMQService rabbitMQService)
    //    {
    //        _rabbitMQService = rabbitMQService;
    //    }

    //    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        _rabbitMQService.PublishDataFromSourceTable(_queueName, dataContext, source);

    //        // throw new NotImplementedException();
    //    }
    //}
}
