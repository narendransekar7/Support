namespace SS.Base.Domain.Messages.Ticket;

public record SlaReserved
{
    public Guid TicketId { get; init; }
}
