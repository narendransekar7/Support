using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Roles;
using UserManagement.User;

namespace UserManagement.UserRoles
{
    public interface IAdminRoles: IBasicUserRoles, IManageCustomerAccount, ITicketEsclation, ITicketAssignment, IMeetingArrangement, ILogWork
    {
        string GetUserRole(Guid UserId);

        bool SetUserRole(Guid UserId, string Role);

        bool CreateAgentAccount(IPersonModel person);




    }
}
