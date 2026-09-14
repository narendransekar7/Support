namespace SS.Base.Domain.Messages.Ticket;

public record AssignEngineerCommand
{
    public Guid TicketId { get; init; }
}
