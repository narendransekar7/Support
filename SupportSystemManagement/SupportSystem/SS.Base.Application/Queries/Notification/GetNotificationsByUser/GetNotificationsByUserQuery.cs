namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Dto;

/// <summary>A user's notification feed: unread count + the newest <see cref="Take"/> notifications.</summary>
public class GetNotificationsByUserQuery : IRequest<NotificationFeedDto>
{
    public Guid UserId { get; set; }
    public int Take { get; set; }

    public GetNotificationsByUserQuery(Guid userId, int take)
    {
        UserId = userId;
        Take = take;
    }
}
