using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Business_Layer.Service
{
    public class RabbitMQPublisher
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _exchange;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            var rabbitMQSettings = configuration.GetSection("RabbitMQ");
            _hostName = rabbitMQSettings["HostName"];
            _userName = rabbitMQSettings["UserName"];
            _password = rabbitMQSettings["Password"];
            _exchange = rabbitMQSettings["Exchange"];
        }

        public void PublishMessage(string routingKey, string message)
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Direct);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: _exchange, routingKey: routingKey, basicProperties: null, body: body);
            Console.WriteLine($"Message Sent: {message}");
        }
    }
}
