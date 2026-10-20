namespace SS.Base.Domain.Interfaces.Messaging;

/// <summary>
/// Broker-independent facts about a delivered message.
/// </summary>
/// <param name="EventId">Id assigned when the message was published; the same on every redelivery.</param>
/// <param name="OccurredAtUtc">When the message was published.</param>
/// <param name="Provider">Which broker delivered it ("Kafka", "RabbitMq", ...).</param>
/// <param name="CorrelationId">Optional correlation id (MassTransit CorrelationId, Kafka message key).</param>
public sealed record EventContext(Guid EventId, DateTime OccurredAtUtc, string Provider, string? CorrelationId = null);
