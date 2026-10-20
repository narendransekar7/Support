using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record AssignEngineerCommand : ITicketMessage
{
    public Guid TicketId { get; init; }
}
