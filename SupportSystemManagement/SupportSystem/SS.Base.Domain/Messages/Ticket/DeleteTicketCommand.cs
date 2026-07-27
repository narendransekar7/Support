namespace SS.Base.Domain.Messages.Ticket;

public record DeleteTicketCommand
{
    public Guid TicketId { get; init; }
}
