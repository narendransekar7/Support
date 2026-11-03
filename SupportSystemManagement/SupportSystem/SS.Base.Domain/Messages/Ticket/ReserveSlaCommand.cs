using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record ReserveSlaCommand : ITicketMessage
{
    public int TicketId { get; init; }
    public string Priority { get; init; }
}
