using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.Ticket;

/// <summary>
/// A ticket passed its resolution due date while still not Closed. Published once per ticket by the
/// SLA breach checker (SS.Functions, an Azure Functions timer trigger); SS.Notification.API and
/// SS.Audit.API react to it.
/// </summary>
public record TicketSlaBreached : ITicketMessage
{
    public int TicketId { get; init; }
    public string Title { get; init; }
    public string Priority { get; init; }
    public DateTime ResolutionDueDate { get; init; }
    public DateTime DetectedAt { get; init; }
    public int CreatedBy { get; init; }
    public int? AssignedTo { get; init; }
}
