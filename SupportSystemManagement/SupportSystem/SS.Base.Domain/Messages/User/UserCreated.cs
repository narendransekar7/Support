using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Domain.Messages.User;

/// <summary>
/// Published by CreateUserHandler once a user is saved. Consumed independently by SS.Email.API (welcome
/// email), SS.Notification.API (welcome notification) and SS.Audit.API (audit trail) - there's no saga.
/// </summary>
public record UserCreated : IUserMessage
{
    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string DisplayName { get; init; }
    public string Role { get; init; }
}
