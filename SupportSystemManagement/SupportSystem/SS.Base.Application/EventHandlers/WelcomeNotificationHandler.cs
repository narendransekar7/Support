using Microsoft.Extensions.Logging;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.User;

namespace SS.Base.Application.EventHandlers;

/// <summary>
/// "Welcome" notification for a newly created user, hosted by SS.Notification.API. The new user sees it
/// in the notification bell after their first sign-in.
/// </summary>
public class WelcomeNotificationHandler : IEventHandler<UserCreated>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WelcomeNotificationHandler> _logger;

    public WelcomeNotificationHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork, ILogger<WelcomeNotificationHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(UserCreated message, EventContext context, CancellationToken cancellationToken)
    {
        // At-least-once delivery: the same UserCreated can arrive twice; create one notification.
        if (await _notificationRepository.ExistsBySourceEventIdAsync(context.EventId, cancellationToken))
        {
            _logger.LogInformation("Welcome notification for message {EventId} (user {UserId}) already exists; skipping duplicate", context.EventId, message.UserId);
            return;
        }

        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = message.UserId,
            Type = NotificationType.Welcome,
            TicketId = null,
            Message = $"Welcome to Support System, {message.DisplayName}! Create a ticket whenever you need help.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            SourceEventId = context.EventId
        };

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created welcome notification {NotificationId} for user {UserId}", notification.NotificationId, message.UserId);
    }
}
