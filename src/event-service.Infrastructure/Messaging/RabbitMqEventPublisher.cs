using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using event_service.Domain.Events;
using event_service.Domain.Ports;

namespace event_service.Infrastructure.Messaging
{
    public class RabbitMqEventPublisher : IDomainEventPublisher
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _exchangeName;

        public RabbitMqEventPublisher(IConnectionFactory connectionFactory, string exchangeName)
        {
            _connectionFactory = connectionFactory;
            _exchangeName = exchangeName;
        }

        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true);

            var payload = JsonConvert.SerializeObject(domainEvent);
            var body = Encoding.UTF8.GetBytes(payload);

            var props = channel.CreateBasicProperties();
            props.Persistent = true;

            channel.BasicPublish(exchange: _exchangeName, routingKey: domainEvent.GetType().Name, basicProperties: props, body: body);

            return Task.CompletedTask;
        }
    }
}
