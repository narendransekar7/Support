using SS.Base.Domain.Entities;

namespace SS.Base.Domain.Interfaces.Repository;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserAsync(Guid userId);
}
