namespace SS.Base.Domain.Interfaces.Messaging;

/// <summary>
/// Publishes a message to whichever broker `Messaging:Provider` selects (Kafka, RabbitMQ, ...).
/// Business code depends only on this port; SS.Base.Messaging implements it per provider.
/// </summary>
public interface IEventPublisher
{
    /// <param name="message">The message contract (a record from SS.Base.Domain.Messages).</param>
    /// <param name="key">
    /// Partition/ordering key. Kafka sends every message with the same key to the same partition,
    /// so one ticket's messages are consumed in order. Providers without keys ignore it.
    /// </param>
    Task PublishAsync<T>(T message, string? key = null, CancellationToken cancellationToken = default) where T : class;
}
