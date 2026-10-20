using MassTransit;
using SS.Base.Application.EventHandlers;
using SS.Base.Domain.Messages.User;
using SS.Base.Messaging.RabbitMq;

namespace SS.Base.Application.Consumers;

/// <summary>
/// RabbitMQ (MassTransit) entry point of the welcome notification, hosted by SS.Notification.API; the logic
/// lives in <see cref="WelcomeNotificationHandler"/>.
/// </summary>
public class WelcomeNotificationConsumer : IConsumer<UserCreated>
{
    private readonly WelcomeNotificationHandler _handler;

    public WelcomeNotificationConsumer(WelcomeNotificationHandler handler) => _handler = handler;

    public Task Consume(ConsumeContext<UserCreated> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
