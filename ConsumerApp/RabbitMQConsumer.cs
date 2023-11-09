using MessageBrokers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


namespace ConsumerApp
{
    public class RabbitMQConsumer
    {
        private readonly RabbitMQService _rabbitMQService;

        public RabbitMQConsumer( RabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }
        public async Task ConsumeDataToTargetTable<T>(string queueName, DbContext dbContext, DbSet<T> targetTable) where T : class, new()
        {
            using (var connection = _rabbitMQService.GetConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
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
                                    // Burada tekrar deneme stratejileri uygulanabilir
                                    var retryAttempts = 3; // Belirli bir sayıda tekrar deneme
                                    var currentAttempt = 0;

                                    while (currentAttempt < retryAttempts)
                                    {
                                        try
                                        {
                                            dbContext.Add(entity);
                                            dbContext.SaveChanges();
                                            break; // İşlem başarılıysa döngüden çık
                                        }
                                        catch (Exception ex)
                                        {
                                            // Hata durumunda tekrar deneme işlemleri
                                            currentAttempt++;
                                            Console.WriteLine("Tekrar deneme {0}. Hata: {1}", currentAttempt, ex.Message);
                                            // Bekleme süresi eklemek için Task.Delay kullanabilirsiniz
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Boş mesaj");
                            }
                        }
                        catch (JsonReaderException ex)
                        {
                            Console.WriteLine("JSON dönüşüm hatası: {0} Hatalı Mesaj: {1}", ex.Message, message);
                        }
                    };
                    channel.BasicConsume(queue: queueName,
                                         autoAck: true,
                                         consumer: consumer);

                    Console.ReadLine();
                }
            }
        }

    }
}
