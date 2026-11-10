using MassTransit;
using SS.Base.Application.EventHandlers;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Messaging.RabbitMq;

namespace SS.Base.Application.Consumers;

/// <summary>
/// RabbitMQ (MassTransit) entry point for SLA-breach notifications, hosted by SS.Notification.API;
/// the logic lives in <see cref="SlaBreachedNotificationHandler"/>.
/// </summary>
public class SlaBreachedNotificationConsumer : IConsumer<TicketSlaBreached>
{
    private readonly SlaBreachedNotificationHandler _handler;

    public SlaBreachedNotificationConsumer(SlaBreachedNotificationHandler handler) => _handler = handler;

    public Task Consume(ConsumeContext<TicketSlaBreached> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
