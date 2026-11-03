using Microsoft.EntityFrameworkCore;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;

namespace SS.Base.Infrastructure.Persistance.MSSQL.Repositories;

public class TicketCreationSagaRepository : GenericRepository<TicketCreationSagaState>, ITicketCreationSagaRepository
{
    private readonly MSSQLDbContext _context;

    public TicketCreationSagaRepository(MSSQLDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<TicketCreationSagaState?> GetByTicketIdAsync(int ticketId) =>
        _context.TicketCreationSagaStates.FirstOrDefaultAsync(s => s.TicketId == ticketId);
}
