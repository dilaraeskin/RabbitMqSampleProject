using RabbitMQ.Client;

namespace MessageBrokers
{
    public class RabbitMQConnectionService
    {
        private readonly RabbitMQSettings _settings;

        public RabbitMQConnectionService(RabbitMQSettings settings)
        {
            _settings = settings;
        }

        public IConnection CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                UserName = _settings.UserName,
                Password = _settings.Password,
                Port = _settings.Port
            };

            var conn = factory.CreateConnection();
            return conn;
        }
    }
}
