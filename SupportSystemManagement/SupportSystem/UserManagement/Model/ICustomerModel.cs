using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Model
{
    public interface ICustomerModel : IPersonModel
    {
        bool IsNormalUser { get; set; }
        bool IsSuperUser { get; set; }

        bool IsLiscensedUser { get; set; }
    }
}
