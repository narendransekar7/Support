using SupportSystem.User.Creation;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Factory
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
