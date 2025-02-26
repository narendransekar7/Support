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
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        private readonly MSSQLDbContext _context;

        public TicketRepository(MSSQLDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByUserAsync(Guid userId)
        {
            return await _context.Tickets.Where(t => t.AssignedTo == userId).ToListAsync();
        }
        
        public async Task<Ticket?> GetByIdWithUpdatesAsync(Guid ticketId)
        {
            return await _context.Tickets
                .Include(t => t.TicketUpdates) // Load TicketUpdates
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
             
            
        }
    }
}
