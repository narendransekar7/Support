using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.User;

namespace UserManagement.UserTypes.Customer
{
    public interface ICustomerModel :IPersonModel
    {
        bool IsNormalUser { get; set; }
        bool IsSuperUser { get; set; }

        bool IsLiscensedUser { get; set; }
    }
}
