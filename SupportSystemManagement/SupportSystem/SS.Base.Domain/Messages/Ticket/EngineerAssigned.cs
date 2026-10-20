using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record EngineerAssigned : ITicketMessage
{
    public Guid TicketId { get; init; }
    public Guid EngineerId { get; init; }
}
