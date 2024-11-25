using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class UpdateTicketStatusCommand
    {
        public Guid TicketId { get; set; }
        public string Status { get; set; }
    }
}
