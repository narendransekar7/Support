using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.Base.Domain.Entities;

namespace SS.Base.Domain.Entities
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ResolutionDueDate { get; set; }
        public DateTime? ResponseDueDate { get; set; }

        // When the SLA breach checker (SS.Functions) found this ticket past ResolutionDueDate and
        // published TicketSlaBreached; null until then. Stops the checker reporting the same ticket twice.
        public DateTime? SlaBreachedAt { get; set; }
        public int CreatedBy { get; set; }
        public TicketStatus Status { get; set; }
        public string Priority { get; set; }
        public int? AssignedTo { get; set; }
        public string Visibility { get; set; }


        // Navigation Property
        public virtual User User { get; set; }


        //One to many relationship acheving using the fluent API
        public ICollection<TicketUpdate> TicketUpdates { get; set; } = new List<TicketUpdate>();

    }
}
