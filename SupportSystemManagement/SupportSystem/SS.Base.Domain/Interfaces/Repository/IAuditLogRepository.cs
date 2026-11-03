using SS.Base.Domain.Entities;

namespace SS.Base.Domain.Interfaces.Repository;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int? ticketId, int? userId, int take, CancellationToken cancellationToken = default);
}
