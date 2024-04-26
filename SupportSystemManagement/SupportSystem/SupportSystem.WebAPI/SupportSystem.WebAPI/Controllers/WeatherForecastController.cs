using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SupportSystem.Backend.Interface.User;
using SupportSystem.Backend.SQLServer.Connection;
using SupportSystem.Backend.SQLServer.Factory;
using SupportSystem.User.Model;
using System.Data.SqlClient;
using SupportSystem.User.Factory;

namespace SupportSystem.WebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        //    [HttpGet]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    var rng = new Random();
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = rng.Next(-20, 55),
        //        Summary = Summaries[rng.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}


        [HttpGet]
        public IActionResult Get()
        {
            string Server = Environment.GetEnvironmentVariable("server");
            string Database = Environment.GetEnvironmentVariable("database");
            string User_Id = Environment.GetEnvironmentVariable("userid");
            string Passowrd = Environment.GetEnvironmentVariable("password");

            if (string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(Database) || string.IsNullOrEmpty(User_Id) || string.IsNullOrEmpty(Passowrd))
            {
                Server = @"DESKTOP-JIV0A8R\SQLEXPRESS";
                Database = "SupportSystem";
                User_Id = "SupportAdmin";
                Passowrd = "NarenSql@123";
            }


            try
            {
                // List to store query results
                var result = new List<object>();

                // SQL query to select data
                string sqlQuery = "SELECT [UserName],[FirstName],[LastName],[DisplayName],[PrimaryEmail],[PrimaryPhoneNumber],[Gender],[Company] ,[CreatedDate] FROM SS_User";
                string _connectionString = "Data Source="+ Server+";Initial Catalog="+ Database + ";User Id="+ User_Id+"; Password="+ Passowrd+ ";" ;
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
                                    FirstName = reader.GetString(0), // Assuming Id is the first column
                                                                    // Add other properties as needed
                                    LastName = reader.GetString(1)
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
