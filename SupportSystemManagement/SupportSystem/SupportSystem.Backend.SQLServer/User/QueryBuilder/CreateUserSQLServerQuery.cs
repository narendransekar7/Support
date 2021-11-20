using SupportSystem.Backend.Interface.User.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.User;

namespace SupportSystem.Backend.SQLServer.User.QueryBuilder
{
    public class CreateUserSQLServerQuery : ICreateUserQuery
    {
        public bool ExecuteUserAddQuery(IPersonModel person)
        {
            return true;
        }
    }
}
