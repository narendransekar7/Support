namespace SS.Base.Domain.Interfaces.Messaging;

/// <summary>
/// A message about one ticket. Lets generic code (Kafka partition key, audit trail) read the
/// ticket id without knowing the concrete message type.
/// </summary>
public interface ITicketMessage
{
    Guid TicketId { get; }
}
