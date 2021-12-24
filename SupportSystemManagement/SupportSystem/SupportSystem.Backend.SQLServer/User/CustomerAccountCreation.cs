using SupportSystem.Backend.Interface.User;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.SQLServer.User
{
    public class CustomerAccountCreation : IAccountCreation
    {

        public CustomerAccountCreation(IPersonModel person)
        {


        }


        public bool CreateAccount()
        {
            throw new NotImplementedException();
        }
    }
}
