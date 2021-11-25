using SupportSystem.Backend.Interface.User;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.SQLServer.User
{
    public class CreateUserSQLServerQuery : ICreateUserQuery
    {
        public bool ExecuteUserAddQuery(IPersonModel person)
        {
            return true;
        }
    }
}
