namespace SS.Base.Domain.Messages.Ticket;

public record SendTicketCreationFailedEmail
{
    public Guid TicketId { get; init; }
    public string Title { get; init; }
    public string CreatedByEmail { get; init; }
    public string CreatedByName { get; init; }
    public string Reason { get; init; }
}
