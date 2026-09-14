namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;

public class GetNotificationsByUserHandler : IRequestHandler<GetNotificationsByUserQuery, IEnumerable<Notification>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsByUserHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<Notification>> Handle(GetNotificationsByUserQuery request, CancellationToken cancellationToken)
    {
        return await _notificationRepository.GetByUserAsync(request.UserId);
    }
}
