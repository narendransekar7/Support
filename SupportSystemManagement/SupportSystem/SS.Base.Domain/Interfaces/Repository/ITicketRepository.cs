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
        Task<IEnumerable<Ticket>> GetTicketsByUserAsync(int userId);
        Task<Ticket?> GetByIdWithUpdatesAsync(int ticketId);

        /// <summary>
        /// Tickets not Closed, past their resolution due date and not yet reported as breached
        /// (oldest due date first, at most <paramref name="take"/>).
        /// </summary>
        Task<List<Ticket>> GetUnreportedSlaBreachesAsync(DateTime now, int take, CancellationToken cancellationToken);
    }
}
