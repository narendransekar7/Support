using SupportManagement.Ticket.Model;
using System;
using System.Collections.Generic;
using System.Text;
using TicketManagement.Updation.Model;

namespace SupportSystem.User.Tasks
{
    public interface IBasicUserRoles
    {
        string UpdateTicket(Guid TicketId, List<ITicketUpdateModel> TicketUpdates);

        string CreateTicket(List<ITicketModel> TicketInfo);

        bool UpdateFirstName(Guid UserId, string FirstName);

        bool UpdateLasttName(Guid UserId, string LastName);

        bool UpdateCountry(Guid UserId, string FirstName);

    }
}
