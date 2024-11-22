using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.Ticket
{
    public interface ITicket
    {
        /// <summary>
        /// Id of the user who creates the ticket.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Title of the ticket
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Ticket Created date.
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Ticket Updated date.
        /// </summary>
        DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Next Action date.
        /// </summary>
        DateTime ResolutionDueDate { get; set; }

        /// <summary>
        /// Next Action date.
        /// </summary>
        DateTime ResponseDueDate { get; set; }

        /// <summary>
        /// Indicates the user who created the ticket.
        /// </summary>
        Guid CreatedBy { get; set; }

        /// <summary>
        /// Indicates the current status of the ticket.
        /// </summary>
        string Status { get; set; }

        /// <summary>
        /// Indicates the current priority of the ticket.
        /// </summary>
        string Priority { get; set; }

        /// <summary>
        /// Can be assigned to User
        /// </summary>
        Guid AssignedTo { get; set; }

        /// <summary>
        /// Ticket can be public, private or internal.
        /// </summary>
        string Visibility { get; set; }

    }
}
