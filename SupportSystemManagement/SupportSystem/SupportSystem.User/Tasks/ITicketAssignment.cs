using SupportSystem.Ticket.Updation;

namespace SupportSystem.User.Tasks;

public interface ITicketAssignment
{
    bool AssignTicket(Guid TicketId, IAssignee Assignee);

}