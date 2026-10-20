using MediatR;
using SS.Base.Domain.Interfaces.Repository;

namespace SS.Base.Application.Commands
{
    public class MarkAllNotificationsReadHandler : IRequestHandler<MarkAllNotificationsReadCommand, int>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkAllNotificationsReadHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
        {
            var unread = (await _notificationRepository.GetUnreadByUserAsync(request.UserId, cancellationToken)).ToList();
            foreach (var notification in unread)
                notification.IsRead = true;

            if (unread.Count > 0)
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            return unread.Count;
        }
    }
}
