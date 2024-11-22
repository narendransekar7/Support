using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.Ticket
{
    public interface ITicketUpdate
    {
        Guid UpdateId { get; set; }
        Guid TicketId { get; set; }
        Guid UpdatedBy { get; set; }
        string Content { get; set; }
        DateTime UpdatedAt { get; set; }

    }
}
