using SupportSystem.Backend.Interface.User;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Creation
{
    public class CustomerCreation : IUserCreation
    {


        dynamic _connection;
        IPersonModel _person;

        public CustomerCreation(IPersonModel person, dynamic connection)
        {
            _person = person;
            _connection = connection;
        }


        public bool CreateUser()
        {

            return true;
        }
    }
}
