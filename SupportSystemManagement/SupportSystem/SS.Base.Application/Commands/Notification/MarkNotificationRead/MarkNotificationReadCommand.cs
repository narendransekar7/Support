using MediatR;

namespace SS.Base.Application.Commands
{
    /// <summary>Marks one of <see cref="UserId"/>'s notifications as read. Returns false if it isn't theirs (or doesn't exist).</summary>
    public class MarkNotificationReadCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid NotificationId { get; set; }
    }
}
