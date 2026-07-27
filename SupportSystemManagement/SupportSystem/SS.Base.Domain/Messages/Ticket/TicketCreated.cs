namespace SS.Base.Domain.Messages.Ticket;

public record TicketCreated
{
    public Guid TicketId { get; init; }
    public string Title { get; init; }
    public string Priority { get; init; }
    public Guid CreatedBy { get; init; }
    public string CreatedByEmail { get; init; }
    public string CreatedByName { get; init; }
}
