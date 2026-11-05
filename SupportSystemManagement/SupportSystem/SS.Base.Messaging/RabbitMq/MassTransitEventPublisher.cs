using MassTransit;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.RabbitMq;

/// <summary>
/// RabbitMQ implementation of <see cref="IEventPublisher"/>: a MassTransit Publish (fan-out exchange
/// per message type). Inside a consumer, the scoped IPublishEndpoint is that consumer's context, so
/// follow-up messages keep the conversation/correlation ids. The key is a Kafka concept and is ignored.
/// </summary>
internal sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

    public Task PublishAsync<T>(T message, string? key = null, CancellationToken cancellationToken = default) where T : class =>
        _publishEndpoint.Publish(message, cancellationToken);
}
