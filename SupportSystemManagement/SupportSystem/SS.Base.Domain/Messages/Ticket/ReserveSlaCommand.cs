using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record ReserveSlaCommand : ITicketMessage
{
    public Guid TicketId { get; init; }
    public string Priority { get; init; }
}
