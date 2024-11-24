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
        public Guid TicketId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime ResolutionDueDate { get; set; }
        public DateTime ResponseDueDate { get; set; }
        public Guid CreatedBy { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public Guid AssignedTo { get; set; }
        public string Visibility { get; set; }


        // Navigation Property
        public virtual User User { get; set; }


        //One to many relationship acheving using the fluent API
        public ICollection<TicketUpdate> TicketUpdates { get; set; } = new List<TicketUpdate>();

    }
}
