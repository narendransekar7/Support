using SupportSystem.Backend.Interface.User;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.SQLServer.User
{
    public class AgentAccountUpdation : IAccountUpdation
    {

        public AgentAccountUpdation()
        {

        }

        public Guid UserId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool UpdateFirstName(string FirstName)
        {
            throw new NotImplementedException();
        }

        public bool UpdateLastName(string LastName)
        {
            throw new NotImplementedException();
        }
    }
}
