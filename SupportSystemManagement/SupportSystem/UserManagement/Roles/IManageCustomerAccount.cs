using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.User;

namespace UserManagement.Roles
{
    public interface IManageCustomerAccount
    {
        bool CreateCustomerAccount(ICustomerModel person);

        bool UpdateCustomerAccount(ICustomerModel person);
    }
}
