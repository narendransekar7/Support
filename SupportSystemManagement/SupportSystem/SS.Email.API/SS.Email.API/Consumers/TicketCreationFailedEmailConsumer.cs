using MassTransit;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Messaging.RabbitMq;
using SS.Email.API.Handlers;

namespace SS.Email.API.Consumers;

/// <summary>
/// RabbitMQ (MassTransit) entry point; the email itself is sent by <see cref="TicketCreationFailedEmailHandler"/>.
/// </summary>
public class TicketCreationFailedEmailConsumer : IConsumer<SendTicketCreationFailedEmail>
{
    private readonly TicketCreationFailedEmailHandler _handler;

    public TicketCreationFailedEmailConsumer(TicketCreationFailedEmailHandler handler) => _handler = handler;

    public Task Consume(ConsumeContext<SendTicketCreationFailedEmail> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
