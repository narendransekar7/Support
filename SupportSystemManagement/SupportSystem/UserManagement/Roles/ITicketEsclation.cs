using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Roles
{
   public interface ITicketEsclation
    {
        bool EsclateTicket(Guid TicketId, string Comment);

        Guid UserId { get; set; }

        bool RemoveTicketFromEsclation(Guid TicketId, string Reason);
    }
}
