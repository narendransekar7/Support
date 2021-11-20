using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.User;
using UserManagement.User.Creation;

namespace UserManagement.UserTypes.Agent
{
    public class AgentCreation : IUserCreation
    {
        public bool CreateUser(IPersonModel person)
        {
            return true;
        }
    }
}
