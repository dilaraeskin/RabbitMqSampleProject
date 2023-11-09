using DataAccessLayer;
using DataContract;
using MessageBrokers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsumerApp
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly DestinationDataContext _destinationDataContext;
        private readonly ILogger<ConsumerHostedService> _logger;
        private static readonly string _queueName = "my_queue";

        public ConsumerHostedService(RabbitMQService rabbitMQService, ILogger<ConsumerHostedService> logger, DestinationDataContext destinationDataContext)
        {
            _rabbitMQService = rabbitMQService;
            _logger = logger;
            _destinationDataContext = destinationDataContext;
        }
        public void ConsumeDataToTargetTable()
        {
            _rabbitMQService.RegisterConsumer<Target>(_queueName, "", (message) => ConsumeQueueMessage(message));
        }
        private void ConsumeQueueMessage(Target message)
        {
            try
            {
                _destinationDataContext.Add(message);
                _destinationDataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Data cannot added : {time}", DateTimeOffset.Now);
            }
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _rabbitMQService.DeclareQueue(_queueName);
            ConsumeDataToTargetTable();

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("ConsumerApp running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000);
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ConsumerApp stopping at: {time}", DateTimeOffset.Now);
            return Task.CompletedTask;
        }
    }
}
