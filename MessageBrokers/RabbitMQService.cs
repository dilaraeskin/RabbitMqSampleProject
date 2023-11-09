using RabbitMQ.Client;

namespace MessageBrokers
{
    public class RabbitMQService
    {
        private IConnection _connection;
        private static RabbitMQConnectionService _rabbitMQConnectionService;
        public RabbitMQSettings _rabbitMQSettings;

        public RabbitMQService(RabbitMQSettings rabbitMQSettings)
        {
            _rabbitMQConnectionService = new RabbitMQConnectionService(rabbitMQSettings);
            var conn = _rabbitMQConnectionService.CreateConnection();
            _rabbitMQSettings = rabbitMQSettings;
        }

        public IConnection GetConnection()
        {
            try
            {
                if (_connection != null && _connection.IsOpen)
                    return _connection;

                try
                {
                    _connection = _rabbitMQConnectionService.CreateConnection();
                    return _connection;
                }
                catch (Exception ex)
                {
                    _connection = null;
                }

                throw new TaskCanceledException("RabbitMQ connection task was cancelled");
            }
            catch (TaskCanceledException e)
            {
                throw new Exception("Some error message");
            }
        }

    }
}