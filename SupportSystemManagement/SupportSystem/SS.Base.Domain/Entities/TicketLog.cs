using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Entities
{
    public class TicketLog
    {
        [Key]
        public Guid LogId { get; set; }
        public Guid TicketId { get; set; }
        public string Action { get; set; }
        public Guid PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
