using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;

namespace SS.Base.Infrastructure.Persistance.MSSQL.Repositories;

public class TicketCreationSagaRepository : GenericRepository<TicketCreationSagaState>, ITicketCreationSagaRepository
{
    public TicketCreationSagaRepository(MSSQLDbContext context) : base(context)
    {
    }
}
