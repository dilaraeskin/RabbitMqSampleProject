using DataAccessLayer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace MessageBrokers
{
    public class RabbitMQService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly RabbitMQSettings _rabbitMQSettings;

        public RabbitMQService(IOptions<RabbitMQSettings> options, ILogger<RabbitMQService> logger)
        {
            _rabbitMQSettings = options.Value;
            _logger = logger;
        }

        private IConnection CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.Host,
                UserName = _rabbitMQSettings.UserName,
                Password = _rabbitMQSettings.Password,
                Port = _rabbitMQSettings.Port
            };
            var conn = factory.CreateConnection();
            return conn;
        }

        private IConnection GetConnection()
        {
            try
            {
                if (_connection != null && _connection.IsOpen)
                    return _connection;

                try
                {
                    _connection = CreateConnection();
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
        public void DeclareQueue(string queueName)
        {
            _connection = GetConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: queueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
        }
        public void Send<T>(T message, string exchange, string routingKey)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            var props = _channel.CreateBasicProperties();
            props.Type = typeof(T).AssemblyQualifiedName;
            _channel.BasicPublish(exchange, routingKey, props, body);

        }
        public void RegisterConsumer<TMessage>(string routingKey, string exchange,
                                           Action<TMessage> consumerCallback) where TMessage : class
        {
            var ebc = new EventingBasicConsumer(_channel);
            var consumerTag = _channel.BasicConsume(queue: routingKey, autoAck: true, consumer: ebc);
            ebc.Received += (model, ea) =>
            {
                try
                {
                    var message = ea.Body.ToArray();
                    var body = JsonConvert.DeserializeObject<TMessage>(Encoding.UTF8.GetString(message));
                    consumerCallback.Invoke(body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

            };
        }

    }
}