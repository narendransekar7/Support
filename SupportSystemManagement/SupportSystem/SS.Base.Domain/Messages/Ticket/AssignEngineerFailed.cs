using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record AssignEngineerFailed : ITicketMessage
{
    public int TicketId { get; init; }
    public string Reason { get; init; }
}
