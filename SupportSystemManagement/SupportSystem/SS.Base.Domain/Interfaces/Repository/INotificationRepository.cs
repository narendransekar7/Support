using SS.Base.Domain.Entities;

namespace SS.Base.Domain.Interfaces.Repository;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserAsync(int userId, int take, CancellationToken cancellationToken = default);
    Task<int> CountUnreadAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUnreadByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySourceEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
}
