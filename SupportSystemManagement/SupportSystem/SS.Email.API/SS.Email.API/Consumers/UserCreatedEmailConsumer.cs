using MassTransit;
using SS.Base.Domain.Messages.User;
using SS.Base.Messaging.RabbitMq;
using SS.Email.API.Handlers;

namespace SS.Email.API.Consumers;

/// <summary>
/// RabbitMQ (MassTransit) entry point; the email itself is sent by <see cref="UserCreatedEmailHandler"/>.
/// </summary>
public class UserCreatedEmailConsumer : IConsumer<UserCreated>
{
    private readonly UserCreatedEmailHandler _handler;

    public UserCreatedEmailConsumer(UserCreatedEmailHandler handler) => _handler = handler;

    public Task Consume(ConsumeContext<UserCreated> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
