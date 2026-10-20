using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record SlaReserved : ITicketMessage
{
    public Guid TicketId { get; init; }
}
