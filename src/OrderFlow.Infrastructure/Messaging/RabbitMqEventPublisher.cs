using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Events;
using RabbitMQ.Client;

namespace OrderFlow.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private const string ExchangeName = "orderflow.events";

    public RabbitMqEventPublisher(IConfiguration configuration, ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;

        try
        {
            var factory = new ConnectionFactory { Uri = new Uri(configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672") };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ connection failed, events will be logged only");
        }
    }

    public Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var routingKey = domainEvent.GetType().Name;
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<object>(domainEvent));

        if (_channel is { IsOpen: true })
        {
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            _channel.BasicPublish(ExchangeName, routingKey, properties, body);
            _logger.LogInformation("Published {EventType} to RabbitMQ", routingKey);
        }
        else
        {
            _logger.LogWarning("RabbitMQ unavailable. Event {EventType} was not published", routingKey);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
