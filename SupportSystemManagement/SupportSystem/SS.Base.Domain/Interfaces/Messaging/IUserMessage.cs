namespace SS.Base.Domain.Interfaces.Messaging;

/// <summary>
/// A message about one user. Lets generic code (partition key, audit trail) read the user id without
/// knowing the concrete message type - the user-side counterpart of <see cref="ITicketMessage"/>.
/// </summary>
public interface IUserMessage
{
    Guid UserId { get; }
}
