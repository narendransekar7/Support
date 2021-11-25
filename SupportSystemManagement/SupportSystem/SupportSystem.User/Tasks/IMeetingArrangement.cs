using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Tasks
{
    public interface IMeetingArrangement
    {
        bool ScheduleMeetingForTicket(Guid TicketId);

        bool ScheduleMeetingForUser(Guid UserId);

        bool ScheduleMeetingForCompany(Guid CompanyId);
    }
}
