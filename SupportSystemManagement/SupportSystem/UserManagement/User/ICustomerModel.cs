using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.User
{
    public interface ICustomerModel :IPersonModel
    {
        bool IsNormalUser { get; set; }
        bool IsSuperUser { get; set; }

        bool IsLiscensedUser { get; set; }
    }
}
