using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.User;

namespace UserManagement.UserTypes.Agent
{
    public interface IAgentModel : IPersonModel
    {
        bool IsAdmin { get; set; }
        bool IsSales { get; set; }

        bool IsInternal { get; set; }

        bool IsCoordinator { get; set; }
    }
}
