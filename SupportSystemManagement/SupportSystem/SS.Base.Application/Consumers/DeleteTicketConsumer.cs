using MassTransit;
using SS.Base.Application.EventHandlers;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Messaging.RabbitMq;

namespace SS.Base.Application.Consumers;

/// <summary>
/// Compensation step: removes a ticket that could not be assigned an engineer.
/// RabbitMQ (MassTransit) entry point; the logic lives in <see cref="DeleteTicketHandler"/>, which the Kafka
/// provider calls directly. Keeping this class keeps its RabbitMQ queue name ("DeleteTicket").
/// </summary>
public class DeleteTicketConsumer : IConsumer<DeleteTicketCommand>
{
    private readonly DeleteTicketHandler _handler;

    public DeleteTicketConsumer(DeleteTicketHandler handler) => _handler = handler;

    public Task Consume(ConsumeContext<DeleteTicketCommand> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
