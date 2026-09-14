namespace SS.Base.Domain.Messages.Ticket;

public record EngineerAssigned
{
    public Guid TicketId { get; init; }
    public Guid EngineerId { get; init; }
}
