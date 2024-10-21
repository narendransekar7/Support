using Microsoft.Data.SqlClient;

namespace SupportSystem.User.MSSQL.Backend.Deployment;

 public class Database
    {
        Logger.Logger logger = new Logger.Logger("logs/log.txt");

        private SqlConnection _sqlConnection;
        public Database(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }


        public bool CreateMetaDatabase(string databaseName)
        {
            try
            {
                // Check if the database exists
                if (!DoesDatabaseExist(_sqlConnection, databaseName))
                {
                    // Create the database if it doesn't exist
                    CreateDatabase(_sqlConnection, databaseName);
                    Console.WriteLine("Database created successfully.");
                    logger.Log("Database created successfully.");
                }
                else
                {
                    Console.WriteLine("Database already exists.");
                    logger.Log("Database already exists.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("DB failed");
                logger.Log($"Error: {ex.Message}");
                logger.Log("DB failed");
            }
            return false;
        }


        static bool DoesDatabaseExist(SqlConnection connection, string databaseName)
        {
            // Query to check if the database exists
            string query = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        static void CreateDatabase(SqlConnection connection, string databaseName)
        {
            // Create the database using T-SQL command
            string createDatabaseQuery = $"CREATE DATABASE [{databaseName}]";

            using (SqlCommand command = new SqlCommand(createDatabaseQuery, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine("DB Reached");
            }
        }

    }