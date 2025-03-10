﻿namespace SupportSystem.User.MSSQL.Backend.Deployment;

using Microsoft.Data.SqlClient;
using SupportSystem.Logger;

public class UserTable
{
    private SqlConnection _sqlConnection;
    Logger logger = new Logger("logs/log.txt");

    public UserTable(SqlConnection sqlConnection)
    {
        _sqlConnection = sqlConnection;
    }

    public bool Create()
    {
            
        try
        {
            string createUserTableQuery = @" CREATE TABLE SS_User (
                                                        UserId UNIQUEIDENTIFIER PRIMARY KEY,
                                                        UserName NVARCHAR(255),
                                                        FirstName NVARCHAR(255),
                                                        LastName NVARCHAR(255),
                                                        DisplayName NVARCHAR(255),
                                                        PrimaryEmail NVARCHAR(255),
                                                        PrimaryPhoneNumber NVARCHAR(20),
                                                        Gender NVARCHAR(10),
                                                        Company NVARCHAR(255),
                                                        CreatedDate DATETIME
                                                        )";
            SqlCommand commandTable = new SqlCommand(createUserTableQuery, _sqlConnection);
            commandTable.ExecuteNonQuery();

            logger.Log("Table Reached");
            Console.WriteLine("Table Reached");
            return true;
               
        }
        catch(Exception ex)
           
        {
            Console.WriteLine($"Error: {ex.Message}");
            logger.Log($"Error: {ex.Message}");
            return false;

                
        }

    }
}