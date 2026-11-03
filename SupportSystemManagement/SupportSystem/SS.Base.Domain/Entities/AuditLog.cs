namespace SS.Base.Domain.Entities;

/// <summary>
/// One message the audit service saw on the bus, stored as-is (append-only). Written by
/// SS.Audit.API; EventId is unique, so a redelivered message is recorded only once.
/// </summary>
public class AuditLog
{
    public Guid AuditLogId { get; set; }
    public Guid EventId { get; set; }
    public string EventType { get; set; }
    public int? TicketId { get; set; }
    public int? UserId { get; set; }   // the user the message is about (IUserMessage), e.g. UserCreated
    public int? ActorId { get; set; }
    public string Provider { get; set; }
    public string Payload { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}
