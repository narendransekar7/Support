using SupportSystem.Backend.Interface.Connection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using SupportSystem.Backend.SQLServer.Factory;

namespace SupportSystem.Backend.SQLServer.Connection
{
    public class SQLServerConnection : ISQLServerConnection
    {
        IParameterModel _parameters = SQLSeverBackendFactory.CreateParameterModel();
        public SqlConnection _sqlConnection;

        Logger.Logger logger = new Logger.Logger("logs/log.txt");

        public SQLServerConnection(IParameterModel parameters)
        {
            _parameters.Server = parameters.Server;
            _parameters.Database = parameters.Database;
            _parameters.User_Id = parameters.User_Id;
            _parameters.Password = parameters.Password;
        }

        public SQLServerConnection(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        public SqlConnection UpdateDBNameInConnection(string newDatabaseName)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_sqlConnection.ConnectionString);
                builder.InitialCatalog = newDatabaseName;
                _sqlConnection.Close();
                _sqlConnection.ConnectionString = builder.ConnectionString;
                _sqlConnection.Open();
                logger.Log("updateconnection success");
                Console.WriteLine("updateconnection success");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("updateconnection failed");
                logger.Log($"Error: {ex.Message}");
                logger.Log("updateconnection failed");

            }

            return _sqlConnection;

           

        }
        public SqlConnection CreateConnection()
        {
            //SqlConnection connection = new SqlConnection(@"Data Source=" + _parameters.Server + ";Initial Catalog= " + _parameters.Database + ";Persist Security Info=True;User ID=" + _parameters.User_Id + ";Password=" + _parameters.Password);

            SqlConnection connection = new SqlConnection(@"Data Source=" + _parameters.Server + ";Persist Security Info=True;User ID=" + _parameters.User_Id + ";Password=" + _parameters.Password);


            try
            {
                    connection.Open();

                }
                catch(Exception ex)
                {
                logger.Log($"Error: {ex.Message}");

            }

            return connection;

        }
    }
}
