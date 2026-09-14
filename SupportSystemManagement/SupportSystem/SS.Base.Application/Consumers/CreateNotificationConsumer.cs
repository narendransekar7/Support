using MassTransit;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.Consumers;

/// <summary>
/// Independent "Create Notification" branch — reacts to TicketCreated directly,
/// not gated by the assign-engineer saga (see diagram: this runs in parallel
/// with the Assign Engineer -> Reserve SLA branch).
/// </summary>
public class CreateNotificationConsumer : IConsumer<TicketCreated>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNotificationConsumer(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<TicketCreated> context)
    {
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = context.Message.CreatedBy,
            TicketId = context.Message.TicketId,
            Message = $"Your ticket \"{context.Message.Title}\" has been created and is being processed.",
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
