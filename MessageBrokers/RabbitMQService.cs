using DataAccessLayer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

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
        public void ConsumeData<T>(DestinationDataContext _destinationDataContext, string exchange, string routingKey)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Gelen Mesaj: {0}", message);
                        var entity = JsonConvert.DeserializeObject<T>(message, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        });

                        if (entity != null)
                        {
                            try
                            {
                                _destinationDataContext.Add(entity);
                                _destinationDataContext.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation("Datas not added to DataContext: {time}", DateTimeOffset.Now);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("ConsumeData empty messages: {time}", DateTimeOffset.Now);
                    }
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogInformation("ConsumeData running at: {time}", DateTimeOffset.Now);
                }
            };
            _channel.BasicConsume(queue: routingKey,
                                 autoAck: true,
                                 consumer: consumer);

        }

    }
}