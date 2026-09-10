namespace PartnerIntegration.Application.Interfaces;

/// <summary>
/// Abstraction over the message broker used to publish verified transactions.
/// The Application layer only knows it can "publish a message to a queue" —
/// it has no knowledge of RabbitMQ specifically.
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        string queueName,
        TMessage message,
        CancellationToken cancellationToken) where TMessage : class;
}
