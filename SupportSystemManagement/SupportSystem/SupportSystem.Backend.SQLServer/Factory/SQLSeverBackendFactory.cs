using SupportSystem.Backend.Interface.User;
using SupportSystem.Backend.SQLServer.User;
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
