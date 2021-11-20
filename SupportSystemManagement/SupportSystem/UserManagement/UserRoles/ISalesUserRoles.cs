using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Roles;

 namespace UserManagement.UserRoles
{
    public interface ISalesUserRoles : IBasicUserRoles, IManageCustomerAccount, IManageInternalNotes, ITicketEsclation, ILogWork
    {

    }
}
