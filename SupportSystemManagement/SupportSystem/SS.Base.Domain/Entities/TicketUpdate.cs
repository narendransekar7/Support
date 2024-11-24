using SS.Base.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Entities
{
    public class TicketUpdate
    {
        public Guid UpdateId { get; set; }
        public Guid TicketId { get; set; }
        public Guid UpdatedBy { get; set; }
        public string Content { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual User UpdatedByUser { get; set; } = null!;
    }
}
