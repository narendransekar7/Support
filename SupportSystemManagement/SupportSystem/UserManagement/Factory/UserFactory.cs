using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.User;
using UserManagement.User.Creation;
using UserManagement.UserTypes.Agent;
using UserManagement.UserTypes.Customer;

namespace UserManagement.Factory
{
    public static class UserFactory
    {
        public static IUserCreation CreateAgent()
        {
            return new AgentCreation();
        }
        public static IUserCreation CreateCustomer()
        {
            return new CustomerCreation();
        }

        public static IPersonModel GetPersonModel()
        {

            return new PersonModel();
        }

    }
}
