using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PartnerIntegration.Application.Interfaces;
using RabbitMQ.Client;

namespace PartnerIntegration.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of IMessagePublisher. Declares the target queue as
/// durable so messages survive a broker restart, and publishes persistent
/// messages so they survive being queued to disk.
/// </summary>
public sealed class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly object _channelLock = new();
    private IModel? _channel;

    public RabbitMqPublisher(
        IRabbitMqConnectionProvider connectionProvider,
        ILogger<RabbitMqPublisher> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    public Task PublishAsync<TMessage>(
        string queueName,
        TMessage message,
        CancellationToken cancellationToken) where TMessage : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        var channel = GetOrCreateChannel();

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "Published message to queue {QueueName}, size {Size} bytes",
            queueName, body.Length);

        return Task.CompletedTask;
    }

    private IModel GetOrCreateChannel()
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        lock (_channelLock)
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            _channel?.Dispose();
            var connection = _connectionProvider.GetOrCreateConnection();
            _channel = connection.CreateModel();
            return _channel;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
