using SupportSystem.Backend.Interface.Connection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace SupportSystem.Backend.SQLServer.Connection
{
    public class SQLServerConnection : ISQLServerConnection, IConnection
    {
        IParameterModel _parameters;


        public SQLServerConnection(IParameterModel parameters)
        {
            _parameters.Server = parameters.Server;
            _parameters.Database = parameters.Database;
            _parameters.User_Id = parameters.User_Id;
            _parameters.Password = parameters.Password;
        }


        public SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(@"Data Source=" + _parameters.Server + ";Initial Catalog= " + _parameters.Database + ";Persist Security Info=True;User ID=" + _parameters.User_Id + ";Password=" + _parameters.Password);
                try
                {
                    connection.Open();
                }
                catch(Exception e)
                {
                }

            return connection;

        }
    }
}
