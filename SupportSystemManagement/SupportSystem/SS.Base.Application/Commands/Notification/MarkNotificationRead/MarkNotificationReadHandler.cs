using MediatR;
using SS.Base.Domain.Interfaces.Repository;

namespace SS.Base.Application.Commands
{
    public class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, bool>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkNotificationReadHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
            // Someone else's notification looks the same as a missing one - no probing other users' ids.
            if (notification == null || notification.UserId != request.UserId)
                return false;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            return true;
        }
    }
}
