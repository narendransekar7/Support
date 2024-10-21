namespace SupportSystem.User.MSSQL.Backend.Factory;

using SupportSystem.User.MSSQL.Backend.Connection;
using SupportSystem.User.MSSQL.Backend.Deployment;
using SupportSystem.User.Backend.Interface.User;
using SupportSystem.User.MSSQL.Backend.User;
using Microsoft.Data.SqlClient;
using SupportSystem.User.Model;

public static class SQLSeverBackendFactory
{
    #region Deployment
    public static bool CreateMetaData(IParameterModel parameters)
    {

        return new MetaData(parameters).Create();
    }

    public static bool CreateMetaDatabase(SqlConnection sqlConnection,string database)
    {

        return new Database(sqlConnection).CreateMetaDatabase(database);
    }

    public static bool CreateMetaUserTable(SqlConnection sqlConnection)
    {

        return new UserTable(sqlConnection).Create();
    }



    #endregion

    #region Connection
    public static SqlConnection CreateSQLServerConnection(IParameterModel parameters)
    {
        return new SQLServerConnection(parameters).CreateConnection();
    }

    public static SqlConnection UpdateDatabaseNameInConnection(SqlConnection sqlConnection,string newDatabaseName)
    {
        return new SQLServerConnection(sqlConnection).UpdateDBNameInConnection(newDatabaseName);

    }

    public static IParameterModel CreateParameterModel()
    {
        return new ParameterModel();
    }

    #endregion


    #region User
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
    #endregion

}