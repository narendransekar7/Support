using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record EngineerAssigned : ITicketMessage
{
    public int TicketId { get; init; }
    public int EngineerId { get; init; }
}
