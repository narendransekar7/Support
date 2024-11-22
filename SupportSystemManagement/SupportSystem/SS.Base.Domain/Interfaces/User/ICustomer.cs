using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.User
{
    public interface ICustomer : IUser
    {
        bool IsNormalUser { get; set; }
        bool IsSuperUser { get; set; }

        bool IsLiscensedUser { get; set; }
    }
}
