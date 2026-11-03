using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record SlaReserved : ITicketMessage
{
    public int TicketId { get; init; }
}
