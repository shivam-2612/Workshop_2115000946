using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Business_Layer.Service
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private readonly string _queueName = "AddressBookQueue";
        private readonly string _exchange;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var rabbitMQSettings = configuration.GetSection("RabbitMQ");
            _hostName = rabbitMQSettings["HostName"] ?? "localhost";
            _userName = rabbitMQSettings["UserName"] ?? "guest";
            _password = rabbitMQSettings["Password"] ?? "guest";
            _exchange = rabbitMQSettings["Exchange"] ?? "AddressBookExchange";

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName ?? "localhost",
                    UserName = _userName ?? "guest",
                    Password = _password ?? "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                if (_channel == null)
                {
                    _logger.LogError("RabbitMQ Channel creation failed!");
                    return;
                }

                _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Direct);
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: _queueName, exchange: _exchange, routingKey: "user.registered");
                _channel.QueueBind(queue: _queueName, exchange: _exchange, routingKey: "contact.added");

                _logger.LogInformation("RabbitMQ Consumer initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"RabbitMQ Connection Error: {ex.Message}");
                _connection = null;
                _channel = null;
            }
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            if (_channel == null)
            {
                _logger.LogError("RabbitMQ Consumer failed to start because the channel is null.");
                return Task.CompletedTask;  // Exit gracefully
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"[RabbitMQ] Received Message: {message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing RabbitMQ message: {ex.Message}");
                }
            };

            _logger.LogInformation("RabbitMQ Consumer started listening...");

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }


        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
