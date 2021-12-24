﻿using SupportSystem.Backend.Interface.User;
using SupportSystem.User.Creation;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Factory
{
    public static class UserFactory
    {
        public static bool CreateAgent(IAccountCreation accountCreation)
        {
            return new AgentCreation(accountCreation).CreateUser();
        }
        public static IUserCreation CreateCustomer(IPersonModel person, dynamic Connection)
        {
            return new CustomerCreation(person, Connection);
        }

        public static IPersonModel GetPersonModel()
        {

            return new PersonModel();
        }

    }
}
