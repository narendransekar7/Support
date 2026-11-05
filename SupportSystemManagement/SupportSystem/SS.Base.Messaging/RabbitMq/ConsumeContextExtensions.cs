using MassTransit;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.RabbitMq;

public static class ConsumeContextExtensions
{
    /// <summary>
    /// The broker-neutral <see cref="EventContext"/> for a MassTransit delivery, so a MassTransit
    /// consumer can hand its message to an <see cref="IEventHandler{T}"/>.
    /// </summary>
    public static EventContext ToEventContext(this ConsumeContext context) => new(
        context.MessageId ?? Guid.NewGuid(),
        context.SentTime ?? DateTime.UtcNow,
        nameof(MessagingProvider.RabbitMq),
        context.CorrelationId?.ToString());
}
