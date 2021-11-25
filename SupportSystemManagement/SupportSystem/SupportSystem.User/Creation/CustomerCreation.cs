using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Creation
{
    public class CustomerCreation : IUserCreation
    {

        public CustomerCreation()
        {

        }


        public bool CreateUser(IPersonModel person)
        {

            return true;
        }
    }
}
