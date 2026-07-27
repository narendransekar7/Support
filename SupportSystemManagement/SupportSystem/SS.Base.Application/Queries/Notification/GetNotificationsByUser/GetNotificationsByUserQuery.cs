namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;

public class GetNotificationsByUserQuery : IRequest<IEnumerable<Notification>>
{
    public Guid UserId { get; set; }

    public GetNotificationsByUserQuery(Guid userId)
    {
        UserId = userId;
    }
}
