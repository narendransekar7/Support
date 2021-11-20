using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Roles;

namespace UserManagement.UserRoles
{
    public interface ICustomerRoles: IBasicUserRoles, ITicketEsclation,ITicketAssignment
    {


    }
}
