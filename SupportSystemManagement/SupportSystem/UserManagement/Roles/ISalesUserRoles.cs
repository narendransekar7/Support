using SupportSystem.User.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Roles
{
    public interface ISalesUserRoles : IBasicUserRoles, IManageCustomerAccount, IManageInternalNotes, ITicketEsclation, ILogWork
    {

    }
}
