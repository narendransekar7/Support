using Microsoft.Extensions.Logging;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.EventHandlers;

/// <summary>
/// Hosted by SS.Notification.API: tells the assigned engineer (or, for an unassigned ticket, its
/// creator) that the ticket has breached its resolution SLA.
/// </summary>
public class SlaBreachedNotificationHandler : IEventHandler<TicketSlaBreached>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SlaBreachedNotificationHandler> _logger;

    public SlaBreachedNotificationHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork, ILogger<SlaBreachedNotificationHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(TicketSlaBreached message, EventContext context, CancellationToken cancellationToken)
    {
        // At-least-once delivery: one notification per message, however often it is redelivered.
        if (await _notificationRepository.ExistsBySourceEventIdAsync(context.EventId, cancellationToken))
        {
            _logger.LogInformation("SLA-breach notification for message {EventId} (ticket {TicketId}) already exists; skipping duplicate", context.EventId, message.TicketId);
            return;
        }

        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = message.AssignedTo ?? message.CreatedBy,
            Type = NotificationType.SlaBreached,
            TicketId = message.TicketId,
            Message = $"Ticket \"{message.Title}\" ({message.Priority}) has passed its resolution due date of {message.ResolutionDueDate:g}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            SourceEventId = context.EventId
        };

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created SLA-breach notification {NotificationId} for ticket {TicketId}, user {UserId}",
            notification.NotificationId, message.TicketId, notification.UserId);
    }
}
