using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class ModifyTicketUpdateCommand
    {
        public Guid TicketId { get; set; }
        public Guid UpdatedBy { get; set; }
        public string Content { get; set; }
    }
}
