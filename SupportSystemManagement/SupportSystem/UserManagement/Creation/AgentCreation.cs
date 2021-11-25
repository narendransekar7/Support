using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Creation
{
    public class AgentCreation : IUserCreation
    {
        public bool CreateUser(IPersonModel person)
        {
            return true;
        }
    }
}
