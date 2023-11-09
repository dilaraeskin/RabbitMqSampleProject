using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace MessageBrokers
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var rabbitJsonString = File.ReadAllText("rabbitMQSettings.json");
            var rabbitData = JsonSerializer.Deserialize<Settings>(rabbitJsonString);
            var services = new ServiceCollection();
            ConfigureServices(services, rabbitData.RabbitMQSettings);
        }

        private void ConfigureServices(IServiceCollection services, RabbitMQSettings rabbitMQSettings)
        {
            services.AddSingleton(rabbitMQSettings);
            services.AddSingleton<RabbitMQConnectionService>();
        }
    }

    public class Settings
    {
        public RabbitMQSettings RabbitMQSettings { get; set; }
    }
}
