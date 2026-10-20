namespace SS.Base.Domain.Interfaces.Messaging;

/// <summary>
/// Handles one message type, whatever broker delivered it. Every provider (a Kafka consumer
/// worker, a MassTransit consumer for RabbitMQ, ...) ends up calling the same handler, so the
/// business logic is written once.
///
/// Delivery is at-least-once with every provider: a handler can see the same message twice
/// (e.g. after a crash before the offset/ack was stored), so it must be safe to repeat.
/// <see cref="EventContext.EventId"/> is stable across redeliveries and is the de-duplication key.
/// </summary>
public interface IEventHandler<in T> where T : class
{
    Task HandleAsync(T message, EventContext context, CancellationToken cancellationToken);
}
