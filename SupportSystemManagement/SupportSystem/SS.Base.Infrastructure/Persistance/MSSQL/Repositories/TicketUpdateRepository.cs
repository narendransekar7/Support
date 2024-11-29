using Microsoft.EntityFrameworkCore;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Infrastructure.Persistance.MSSQL.Repositories
{
    public class TicketUpdateRepository: GenericRepository<TicketUpdate>, ITicketUpdateRepository
    {
        private readonly MSSQLDbContext _context;

        public TicketUpdateRepository(MSSQLDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
