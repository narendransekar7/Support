using SupportSystem.Backend.Interface.User;
using SupportSystem.Backend.SQLServer.Connection;
using SupportSystem.Backend.SQLServer.User;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SupportSystem.Backend.SQLServer.Factory
{
    public static class SQLSeverBackendFactory
    {

        public static SqlConnection CreateSQLServerConnection(IParameterModel parameters)
        {
            return new SQLServerConnection(parameters).CreateConnection();
        }


        public static IParameterModel CreateParameterModel()
        {
            return new ParameterModel();
        }

        public static IAccountCreation CreateAgentAccount(SqlConnection sqlConnection, IPersonModel personModel)
        {
            return new AgentAccountCreation(sqlConnection, personModel);
        }

        public static IAccountCreation CreateCustomerAccount(IPersonModel person)
        {

            return new CustomerAccountCreation(person);
        }

        public static IAccountUpdation UpdateAgentAccount()
        {

            return new AgentAccountUpdation();
        }

        public static IAccountUpdation UpdateCustomerAccount()
        {

            return new CustomerAccountUpdation();
        }
    }
}
