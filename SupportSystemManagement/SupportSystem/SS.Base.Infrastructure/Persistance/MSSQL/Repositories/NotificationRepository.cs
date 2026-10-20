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

    public async Task<IEnumerable<Notification>> GetByUserAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public Task<bool> ExistsBySourceEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return _context.Notifications.AnyAsync(n => n.SourceEventId == eventId, cancellationToken);
    }
}
