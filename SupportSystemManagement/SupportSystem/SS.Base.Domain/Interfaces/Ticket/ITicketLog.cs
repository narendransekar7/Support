using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.Ticket
{
    public interface ITicketLog
    {
        Guid LogId { get; set; }
        Guid TicketId { get; set; }
        string Action { get; set; }
        Guid PerformedBy { get; set; }
        DateTime PerformedAt { get; set; }
        string OldValue { get; set; }
        string NewValue { get; set; }

    }
}
