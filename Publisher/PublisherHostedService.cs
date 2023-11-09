using DataAccessLayer;
using DataContract;
using MessageBrokers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PublisherApp
{
    public class PublisherHostedService : IHostedService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly SourceDataContext _sourceDataContext;
        private readonly ILogger<PublisherHostedService> _logger;
        private static readonly string _queueName = "my_queue";
        private static readonly int _interval = 10000;
        private List<Source> _lastSourceTableState = new List<Source>();

        public PublisherHostedService(RabbitMQService rabbitMQService, ILogger<PublisherHostedService> logger, SourceDataContext sourceDataContext)
        {
            _rabbitMQService = rabbitMQService;
            _logger = logger;
            _sourceDataContext = sourceDataContext;
        }
        public async Task PublishDataFromSourceTable() 
        {
            var sourceTable = _sourceDataContext.Set<Source>();

            var currentSourceTableState = sourceTable.ToList<Source>();

            var differencesSourceTable = currentSourceTableState.Except(_lastSourceTableState);

            foreach (var difference in differencesSourceTable)
            {
                _rabbitMQService.Send(difference, "", _queueName);
            }
            _lastSourceTableState = currentSourceTableState;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // queue yarat
            _rabbitMQService.DeclareQueue(_queueName);

            //// periyodik işlem başlat
            var timer = new System.Timers.Timer(_interval);
            timer.Elapsed += async (sender, e) => await PublishDataFromSourceTable();
            timer.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Publisher running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000);
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Publisher stopping at: {time}", DateTimeOffset.Now);
            return Task.CompletedTask;
        }
    }
}
