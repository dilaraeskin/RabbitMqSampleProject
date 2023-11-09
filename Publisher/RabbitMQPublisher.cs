using MessageBrokers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Publisher
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQService _rabbitMQService;

        public RabbitMQPublisher(RabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }
        public void PublishDataFromSourceTable<T>(string queueName, DbContext dbContext, DbSet<T> sourceTable) where T : class
        {
            //Belli zaman aralığı içerisinde tabloyu al verileri karşılaştır yeni veya güncellenmiş olanları publish et
            using (var connection = _rabbitMQService.GetConnection())
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
                        var message = JsonConvert.SerializeObject(data);
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "",
                                             routingKey: queueName,
                                             basicProperties: null,
                                             body: body);
                    }
                }
            }
        }

    }
}
