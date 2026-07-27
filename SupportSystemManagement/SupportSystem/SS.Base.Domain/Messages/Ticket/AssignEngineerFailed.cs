namespace SS.Base.Domain.Messages.Ticket;

public record AssignEngineerFailed
{
    public Guid TicketId { get; init; }
    public string Reason { get; init; }
}
