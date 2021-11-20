using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.User;
using UserManagement.UserTypes.Customer;

namespace UserManagement.Roles
{
    public interface IManageCustomerAccount
    {
        bool CreateCustomerAccount(ICustomerModel person);

        bool UpdateCustomerAccount(ICustomerModel person);
    }
}
