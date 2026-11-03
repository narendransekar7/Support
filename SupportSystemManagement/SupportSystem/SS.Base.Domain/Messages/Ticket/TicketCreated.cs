using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

public record TicketCreated : ITicketMessage
{
    public int TicketId { get; init; }
    public string Title { get; init; }
    public string Priority { get; init; }
    public int CreatedBy { get; init; }
    public string CreatedByEmail { get; init; }
    public string CreatedByName { get; init; }
}
