using SupportSystem.Backend.Interface.User.QueryBuilder;
using SupportSystem.Backend.SQLServer.User.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.SQLServer.Factory
{
    public static class SQLSeverBackendFactory
    {

        public static ICreateUserQuery CreateUserQuery()
        {

            return new CreateUserSQLServerQuery();
        }

    }
}
