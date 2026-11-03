using MediatR;

namespace SS.Base.Application.Commands
{
    /// <summary>Marks all of <see cref="UserId"/>'s notifications as read. Returns how many changed.</summary>
    public class MarkAllNotificationsReadCommand : IRequest<int>
    {
        public int UserId { get; set; }
    }
}
