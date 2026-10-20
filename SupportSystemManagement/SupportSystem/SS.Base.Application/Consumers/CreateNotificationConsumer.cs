using MassTransit;
using SS.Base.Application.EventHandlers;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Messaging.RabbitMq;

namespace SS.Base.Application.Consumers;

/// <summary>
/// Independent "Create Notification" branch - reacts to TicketCreated directly, not gated by the
/// assign-engineer saga. Hosted by SS.Notification.API.
/// RabbitMQ (MassTransit) entry point; the logic lives in <see cref="CreateNotificationHandler"/>, which the Kafka
/// provider calls directly. Keeping this class keeps its RabbitMQ queue name ("CreateNotification").
/// </summary>
public class CreateNotificationConsumer : IConsumer<TicketCreated>
{
    private readonly CreateNotificationHandler _handler;

    public CreateNotificationConsumer(CreateNotificationHandler handler) => _handler = handler;

    public Task Consume(ConsumeContext<TicketCreated> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
