using Microsoft.EntityFrameworkCore;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;

namespace SS.Base.Infrastructure.Persistance.MSSQL.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    private readonly MSSQLDbContext _context;

    public NotificationRepository(MSSQLDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetByUserAsync(Guid userId, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsBySourceEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return _context.Notifications.AnyAsync(n => n.SourceEventId == eventId, cancellationToken);
    }
}
