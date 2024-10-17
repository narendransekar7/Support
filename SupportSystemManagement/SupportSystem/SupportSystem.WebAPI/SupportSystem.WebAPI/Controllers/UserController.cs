using Microsoft.AspNetCore.Mvc;
using SupportSystem.Backend.Interface.User;
using SupportSystem.Backend.SQLServer.Connection;
using SupportSystem.Backend.SQLServer.Factory;
using SupportSystem.User.Factory;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SupportSystem.WebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        [HttpPost("createagentuser")]
        public IActionResult CreateAgentUser(PersonModel Person)
        {

            //PersonModel Person = new PersonModel();

            //Consider it as stored in some JSON or XML in the application.
            string Server = Environment.GetEnvironmentVariable("server");
            string Database = Environment.GetEnvironmentVariable("database");
            string User_Id = Environment.GetEnvironmentVariable("userid");
            string Passowrd = Environment.GetEnvironmentVariable("password");

            // below local manchine valsues should be taken from app settings.json
            if (string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(Database) || string.IsNullOrEmpty(User_Id) || string.IsNullOrEmpty(Passowrd))
            { 
                Server = @".";
                Database = "SupportSystem";
                User_Id = "testnaren";
                Passowrd = "Naren@123";
            }
            
            //Common parameter creation.
            IParameterModel Parameters = new ParameterModel();
            Parameters.Server = Server;
            Parameters.User_Id = User_Id;
            Parameters.Password = Passowrd;
            Parameters.Database = Database;

            IAccountCreation accountCreation;

            // IN the below code we should not call the SQL data base directly, which should be cassed depened on bac end database

            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                SqlConnection sqlConnection = SQLSeverBackendFactory.CreateSQLServerConnection(Parameters);
                sqlConnection = SQLSeverBackendFactory.UpdateDatabaseNameInConnection(sqlConnection, Database);

                accountCreation = SQLSeverBackendFactory.CreateAgentAccount(sqlConnection, Person);

                UserFactory.CreateAgent(accountCreation);
                
                // Your logic to create a user
                // For demonstration purposes, let's just return the received data
                return Ok(Person);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetUser")]
        public IActionResult GetUser(string userid)
        {
            string Server = Environment.GetEnvironmentVariable("server");
            string Database = Environment.GetEnvironmentVariable("database");
            string User_Id = Environment.GetEnvironmentVariable("userid");
            string Passowrd = Environment.GetEnvironmentVariable("password");

            if (string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(Database) || string.IsNullOrEmpty(User_Id) || string.IsNullOrEmpty(Passowrd))
            {
                Server = @".";
                Database = "SupportSystem";
                User_Id = "testnaren";
                Passowrd = "Naren@123";
                
            }

           
            
       

            try
            {
                // List to store query results
                var result = new List<object>();

                // SQL query to select data
                string sqlQuery =
                    "SELECT [UserName],[FirstName],[LastName],[DisplayName],[PrimaryEmail],[PrimaryPhoneNumber],[Gender],[Company] ,[CreatedDate] FROM SS_User" +
                    " where UserId = '" + userid+"'";
                string _connectionString = "Data Source=" + Server + ";Initial Catalog=" + Database + ";User Id=" +
                                           User_Id + "; Password=" + Passowrd + ";";
                // Establish a connection to the SQL Server database
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Open the connection
                    connection.Open();

                    // Create a command object with the SQL command and connection
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        // Execute the SQL command and read the data
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Iterate through the result set and add each row to the list
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    // FullName = reader.GetString(0), // Assuming Id is the first column
                                    // Add other properties as needed
                                    
                                    UserName = reader.GetString(0),

                                    FirstName = reader.GetString(1),

                                    LastName  = reader.GetString(2),

                                    DisplayName = reader.GetString(3),

                                    PrimaryEmail = reader.GetString(4),

                                    PrimaryPhoneNumber = reader.GetString(5),

                                    Gender = reader.GetString(6),

                                    Company = reader.GetString(7),
                                    //CreatedDate = reader.GetString(8)
                                    //Name = reader.GetString(1),
                                   // FullName = reader.GetString(0)
                                };
                                result.Add(row);
                            }
                        }
                    }
                }

                // Return the query result
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet]
        public IActionResult GetAllUsers()
        {
            string Server = Environment.GetEnvironmentVariable("server");
            string Database = Environment.GetEnvironmentVariable("database");
            string User_Id = Environment.GetEnvironmentVariable("userid");
            string Passowrd = Environment.GetEnvironmentVariable("password");

            if (string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(Database) || string.IsNullOrEmpty(User_Id) || string.IsNullOrEmpty(Passowrd))
            {
                Server = @".";
                Database = "SupportSystem";
                User_Id = "testnaren";
                Passowrd = "Naren@123";
                
            }
            
            try
            {
                // List to store query results
                var result = new List<object>();

                // SQL query to select data
                string sqlQuery =
                    "SELECT [UserName],[FirstName],[LastName],[DisplayName],[PrimaryEmail],[PrimaryPhoneNumber],[Gender],[Company] ,[CreatedDate] FROM SS_User";
                    
                string _connectionString = "Data Source=" + Server + ";Initial Catalog=" + Database + ";User Id=" +
                                           User_Id + "; Password=" + Passowrd + ";";
                // Establish a connection to the SQL Server database
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Open the connection
                    connection.Open();

                    // Create a command object with the SQL command and connection
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        // Execute the SQL command and read the data
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Iterate through the result set and add each row to the list
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    // FullName = reader.GetString(0), // Assuming Id is the first column
                                    // Add other properties as needed
                                    
                                    UserName = reader.GetString(0),

                                    FirstName = reader.GetString(1),

                                    LastName  = reader.GetString(2),

                                    DisplayName = reader.GetString(3),

                                    PrimaryEmail = reader.GetString(4),

                                    PrimaryPhoneNumber = reader.GetString(5),

                                    Gender = reader.GetString(6),

                                    Company = reader.GetString(7),
                                    //CreatedDate = reader.GetString(8)
                                    //Name = reader.GetString(1),
                                   // FullName = reader.GetString(0)
                                };
                                result.Add(row);
                            }
                        }
                    }
                }

                // Return the query result
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}