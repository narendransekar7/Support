using System;
using System.Collections.Generic;
using System.Text;
using TicketManagement;
using TicketManagement.Updation.Assignment;

namespace UserManagement.Roles
{
    public interface ITicketAssignment
    {
        bool AssignTicket(Guid TicketId, IAssignee Assignee);

    }
}
