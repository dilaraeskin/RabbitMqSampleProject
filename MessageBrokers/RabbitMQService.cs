using DataContract;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace MessageBrokers
{
    public partial class RabbitMQService
    {
        private readonly RabbitMQSettings _settings;
        private IConnection _connection;
        private ConnectionFactory _factory;
        public RabbitMQService(RabbitMQSettings settings)
        {
            _settings = settings;
        }
        public IConnection GetConnection(ConnectionFactory factory)
        {
            _factory = factory;
            try
            {
                if (_connection != default && _connection.IsOpen)
                    return _connection;
                try
                {
                    RabbitMQConnectionService connectionService = new RabbitMQConnectionService(_settings); 
                    _connection = connectionService.CreateConnection(); 

                    return _connection;
                }
                catch (Exception ex)
                {
                    _connection = null;

                    try
                    {
                        Thread.Sleep(3000);
                    }
                    catch (TaskCanceledException e) { }
                }

                throw new TaskCanceledException("RabbitMQ connection task was cancelled");
            }
            catch (TaskCanceledException e)
            {
                throw new Exception("Some error message");
            }
        }
        public void PublishDataFromSourceTable<T>(string queueName, DbContext dbContext, DbSet<T> sourceTable) where T : class
        {
            using (var connection = GetConnection(_factory))
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var sourceData = sourceTable.ToList();
                    foreach (var data in sourceData)
                    {
                        StringBuilder messageBuilder = new StringBuilder();
                        var properties = typeof(T).GetProperties();
                        foreach (var property in properties)
                        {
                            messageBuilder.Append($"{property.Name}: {property.GetValue(data)}, ");
                        }
                        string message = messageBuilder.ToString();
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "",
                                             routingKey: queueName,
                                             basicProperties: null,
                                             body: body);
                    }
                }
            }
        }

        public void ConsumeDataToTargetTable<T>(string queueName, DbContext dbContext, DbSet<T> targetTable) where T : class, new()
        {
            using (var connection = GetConnection(_factory))
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        var targetData = new T();
                        dbContext.Set<T>().Add(targetData);
                        dbContext.SaveChanges();
                    };
                    channel.BasicConsume(queue: queueName,
                                         autoAck: true,
                                         consumer: consumer);

                    while (true)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}