using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.User;
using UserManagement.User.Creation;

namespace UserManagement.UserTypes.Customer
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
