using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;

namespace SS.Base.Infrastructure.Persistance.MSSQL.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    
    private readonly MSSQLDbContext _context;

    public RefreshTokenRepository(MSSQLDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid id)
    {
        return await _context.RefreshTokens.FindAsync(id);
    }
}