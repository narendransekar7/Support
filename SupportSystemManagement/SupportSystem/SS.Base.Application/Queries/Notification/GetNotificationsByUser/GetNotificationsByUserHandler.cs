namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Dto;
using SS.Base.Domain.Interfaces.Repository;

public class GetNotificationsByUserHandler : IRequestHandler<GetNotificationsByUserQuery, NotificationFeedDto>
{
    private const int MaxTake = 100;

    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsByUserHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<NotificationFeedDto> Handle(GetNotificationsByUserQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetByUserAsync(request.UserId, Math.Clamp(request.Take, 1, MaxTake), cancellationToken);
        return new NotificationFeedDto
        {
            UnreadCount = await _notificationRepository.CountUnreadAsync(request.UserId, cancellationToken),
            Items = notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Type = n.Type,
                TicketId = n.TicketId,
                Message = n.Message,
                // Stored as UTC (handlers write DateTime.UtcNow; older rows came from Azure SQL's GETDATE(), also UTC) but
                // read back without a kind - mark it UTC so the JSON carries a "Z" and the browser shows local time.
                CreatedAt = DateTime.SpecifyKind(n.CreatedAt, DateTimeKind.Utc),
                IsRead = n.IsRead
            }).ToList()
        };
    }
}
