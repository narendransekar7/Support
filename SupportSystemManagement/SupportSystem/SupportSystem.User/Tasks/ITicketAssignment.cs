using SupportManagement.Ticket.Updation;
using System;
using System.Collections.Generic;
using System.Text;
using TicketManagement;

namespace SupportSystem.User.Tasks
{
    public interface ITicketAssignment
    {
        bool AssignTicket(Guid TicketId, IAssignee Assignee);

    }
}
