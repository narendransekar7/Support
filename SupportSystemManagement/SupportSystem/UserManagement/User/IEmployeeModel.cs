using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.User
{
    public interface IEmployeeModel : IPersonModel
    {
        bool IsAdmin { get; set; }
        bool IsSales { get; set; }

        bool IsInternal { get; set; }

        bool IsCoordinator { get; set; }
    }
}
