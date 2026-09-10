namespace PartnerIntegration.Application.Interfaces;

/// <summary>
/// Defines a contract for publishing messages to a message broker (e.g., RabbitMQ).
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        string queueName,
        TMessage message,
        CancellationToken cancellationToken) where TMessage : class;
}
