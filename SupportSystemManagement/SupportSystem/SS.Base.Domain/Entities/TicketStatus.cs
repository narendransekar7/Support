using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Entities
{
    public enum TicketStatus
    {
        Open,
        Closed,
        InProgress,
        WaitingForCustomer,
        Hold
    }
}
