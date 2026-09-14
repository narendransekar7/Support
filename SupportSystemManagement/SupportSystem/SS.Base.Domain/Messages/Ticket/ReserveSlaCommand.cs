namespace SS.Base.Domain.Messages.Ticket;

public record ReserveSlaCommand
{
    public Guid TicketId { get; init; }
    public string Priority { get; init; }
}
