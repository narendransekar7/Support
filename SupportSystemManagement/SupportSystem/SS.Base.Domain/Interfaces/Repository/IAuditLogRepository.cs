using SS.Base.Domain.Entities;

namespace SS.Base.Domain.Interfaces.Repository;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetRecentAsync(Guid? ticketId, int take, CancellationToken cancellationToken = default);
}
