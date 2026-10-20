using Microsoft.Extensions.Logging;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.EventHandlers;

/// <summary>
/// "Create Notification" branch, hosted by SS.Notification.API - reacts to TicketCreated directly,
/// not gated by the assign-engineer saga.
/// </summary>
public class CreateNotificationHandler : IEventHandler<TicketCreated>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateNotificationHandler> _logger;

    public CreateNotificationHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork, ILogger<CreateNotificationHandler> logger)
    {
        _logger = logger;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(TicketCreated message, EventContext context, CancellationToken cancellationToken)
    {
        // At-least-once delivery: the same TicketCreated can arrive twice; create one notification.
        if (await _notificationRepository.ExistsBySourceEventIdAsync(context.EventId, cancellationToken))
        {
            _logger.LogInformation("Notification for message {EventId} (ticket {TicketId}) already exists; skipping duplicate", context.EventId, message.TicketId);
            return;
        }

        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = message.CreatedBy,
            TicketId = message.TicketId,
            Message = $"Your ticket \"{message.Title}\" has been created and is being processed.",
            IsRead = false,
            SourceEventId = context.EventId
        };

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created notification {NotificationId} for ticket {TicketId}", notification.NotificationId, notification.TicketId);
    }
}
