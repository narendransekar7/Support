using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class UpdateTicketStatusCommand
    {
        public int TicketId { get; set; }
        public string Status { get; set; }
    }
}
