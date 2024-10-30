using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using SupportSystem.User.Backend.Interface.User;
using SupportSystem.User.Factory;
using SupportSystem.User.Model;
using SupportSystem.User.MSSQL.Backend.Connection;
using SupportSystem.User.MSSQL.Backend.Deployment;
using SupportSystem.User.MSSQL.Backend.Factory;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace SupportSystem.User.API.Controllers;
using Microsoft.AspNetCore.Authorization;

//[Route("[controller]")]
[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{

    private readonly UserDatabase _userDatabasecontext;
    private readonly IPasswordHasher<PersonModel> _passwordHasher;
    
    public UserController(UserDatabase userDatabasecontext, IPasswordHasher<PersonModel> passwordHasher)
    {
        _userDatabasecontext = userDatabasecontext;
        _passwordHasher = passwordHasher;

    }
    
    // Endpoint to validate credentials
    //[AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCredentials([FromBody] LoginModel loginDto)
    {
        var user = await _userDatabasecontext.SS_User.SingleOrDefaultAsync(u => u.PrimaryEmail == loginDto.Email);
        if (user == null)
            return Unauthorized("Invalid credentials.");

        var passwordCheck = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
        if (passwordCheck == PasswordVerificationResult.Success)
            return Ok(new { isValid = true });
        else
            return Unauthorized("Invalid credentials.");
    }
    
    [HttpPost("createagentuser")]
    public IActionResult CreateAgentUser(PersonModel Person)
    {
        IAccountCreation accountCreation;

        // IN the below code we should not call the SQL data base directly, which should be cassed depened on bac end database

        try
        {

            if (_userDatabasecontext.SS_User.Any(u => u.PrimaryEmail == Person.PrimaryEmail))
                return BadRequest("Email is already in use.");
            // Hash the password
            Person.Password = _passwordHasher.HashPassword(Person, Person.Password);

            // Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            accountCreation = SQLSeverBackendFactory.CreateAgent(_userDatabasecontext, Person);

            UserFactory.CreateAgent(accountCreation);

            // Your logic to create a user
            // For demonstration purposes, let's just return the received data
            return Ok("Agent user created sucessfully");
        }
        catch (Exception ex)
        {
            // Log the exception or handle it accordingly
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }


    [HttpGet("getallusers")]
        public IEnumerable<PersonModel> GetAllUsers()
        {
            return _userDatabasecontext.SS_User.ToList();
        }


        [HttpPost("createagentuserold")]
        public IActionResult CreateAgentUserOld(PersonModel Person)
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
            IParameterModel Parameters = new MSSQL.Backend.Connection.ParameterModel();
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
                " where UserId = '" + userid + "'";
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

                                LastName = reader.GetString(2),

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
    public IActionResult GetAllUsersOld()
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

                                LastName = reader.GetString(2),

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
public class LoginModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}