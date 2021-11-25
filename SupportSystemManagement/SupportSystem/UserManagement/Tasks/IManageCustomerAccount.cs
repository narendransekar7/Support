using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Tasks
{
    public interface IManageCustomerAccount
    {
        bool CreateCustomerAccount(ICustomerModel person);

        bool UpdateCustomerAccount(ICustomerModel person);
    }
}
