using Microsoft.AspNetCore.Mvc;
using SupportSystem.Backend.SQLServer.Connection;
using SupportSystem.Backend.SQLServer.Factory;
using SupportSystem.WebAPI.Model.MetaData;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SupportSystem.WebAPI.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class MetaDataController : ControllerBase
    {
        [HttpPost("createmetadata")]
        public IActionResult CreateMetaData(ServerDetail serverDetail)
        {

            switch (serverDetail.Type)
            {
                case DatabaseType.SQLServer:

                    IParameterModel Parameters = new ParameterModel();
                    Parameters.Server = serverDetail.Server;
                    Parameters.User_Id = serverDetail.User_Id;
                    Parameters.Password = serverDetail.Password;
                    Parameters.Database = serverDetail.Database;

                    return Ok(SQLSeverBackendFactory.CreateMetaData(Parameters));


                    //SqlConnection sqlConnection = SQLSeverBackendFactory.CreateSQLServerConnection(Parameters);



                    //connectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";
                    // Code to connect to SQL Server
                    


                case DatabaseType.PostgreSQL:
                    //connectionString = "Host=myServerAddress;Database=myDataBase;Username=myUsername;Password=myPassword;";
                    // Code to connect to PostgreSQL
                    break;

                case DatabaseType.MySQL:
                    //connectionString = "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";
                    // Code to connect to MySQL
                    break;

                default:
                    throw new ArgumentException("Invalid database type specified.");
            }

            return Ok();

        }

    }
}
