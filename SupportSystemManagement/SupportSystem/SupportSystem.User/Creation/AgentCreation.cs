using SupportSystem.Backend.Interface.User;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Creation
{
    public class AgentCreation : IUserCreation
    {
        IAccountCreation _userAccount;


        public AgentCreation(IAccountCreation userAccount)
         //     public AgentCreation(IPersonModel person, dynamic connection, IAccountCreation userAccount)
        {
            _userAccount = userAccount;
        }

        public bool CreateUser()
        {
            _userAccount.CreateAccount();
            return true;
        }
    }
}
