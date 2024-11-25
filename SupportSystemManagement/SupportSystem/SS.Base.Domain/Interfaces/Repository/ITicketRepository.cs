using SS.Base.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.Repository
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        // Add methods specific to Tickets
        Task<IEnumerable<Ticket>> GetTicketsByUserAsync(Guid userId);
    }
}
