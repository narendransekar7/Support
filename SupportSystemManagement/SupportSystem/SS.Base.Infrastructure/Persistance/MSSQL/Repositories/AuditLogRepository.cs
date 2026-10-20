using Microsoft.EntityFrameworkCore;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;

namespace SS.Base.Infrastructure.Persistance.MSSQL.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    private readonly MSSQLDbContext _context;

    public AuditLogRepository(MSSQLDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return _context.AuditLogs.AnyAsync(a => a.EventId == eventId, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(Guid? ticketId, Guid? userId, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking();
        if (ticketId.HasValue)
            query = query.Where(a => a.TicketId == ticketId);
        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        return await query
            .OrderByDescending(a => a.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
