using MassTransit;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Messaging.RabbitMq;
using SS.Email.API.Handlers;

namespace SS.Email.API.Consumers;

/// <summary>
/// RabbitMQ (MassTransit) entry point; the email itself is sent by <see cref="TicketCreatedEmailHandler"/>.
/// </summary>
public class TicketCreatedEmailConsumer : IConsumer<TicketCreated>
{
    private readonly TicketCreatedEmailHandler _handler;

    public TicketCreatedEmailConsumer(TicketCreatedEmailHandler handler) => _handler = handler;

    public Task Consume(ConsumeContext<TicketCreated> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
