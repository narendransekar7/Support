using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record AssignEngineerFailed : ITicketMessage
{
    public Guid TicketId { get; init; }
    public string Reason { get; init; }
}
