using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Business_Layer.Service
{
    public class RabbitMQService 
    {
        private readonly string _hostname;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _exchange;
        private readonly string _queue;

        public RabbitMQService(IConfiguration configuration)
        {
            _hostname = configuration["RabbitMQ:HostName"];
            _userName = configuration["RabbitMQ:UserName"];
            _password = configuration["RabbitMQ:Password"];
            _exchange = configuration["RabbitMQ:Exchange"];
            _queue = configuration["RabbitMQ:Queue"];
        }

        public void PublishMessage(string routingKey, object message)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _hostname,
                    UserName = _userName,
                    Password = _password
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Direct);
                channel.QueueDeclare(queue: _queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queue: _queue, exchange: _exchange, routingKey: routingKey);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: _exchange, routingKey: routingKey, basicProperties: properties, body: body);
            }
            catch(Exception ex)
            {
                Console.WriteLine(  ex.Message);
            }
        }
    }
}
