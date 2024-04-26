using SupportSystem.Backend.Interface.User;
using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SupportSystem.Backend.SQLServer.User
{
    public class AgentAccountCreation : IAccountCreation
    {

        private SqlConnection _sqlConnection;
        private IPersonModel _personModel;


        public AgentAccountCreation(SqlConnection sqlConnection, IPersonModel personModel)
        {
            _sqlConnection = sqlConnection;
            _personModel = personModel;
        }



        public bool CreateAccount()
        {
            Guid UserId= Guid.NewGuid();
            
            try
            {

//                string createUserTableQuery = @"
//            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SS_User')
//            BEGIN
//                -- SQL command to create the table
//              CREATE TABLE SS_User (
//    UserId UNIQUEIDENTIFIER PRIMARY KEY,
//    UserName NVARCHAR(255),
//    FirstName NVARCHAR(255),
//    LastName NVARCHAR(255),
//    DisplayName NVARCHAR(255),
//    PrimaryEmail NVARCHAR(255),
//    PrimaryPhoneNumber NVARCHAR(20),
//    Gender NVARCHAR(10),
//    Company NVARCHAR(255),
//    CreatedDate DATETIME
//)
//            END
//        ";





//                SqlCommand commandTable = new SqlCommand(createUserTableQuery, _sqlConnection);
//                commandTable.ExecuteNonQuery();




                string insertUserQuery = @"INSERT INTO SS_User(UserId,UserName,FirstName,LastName,DisplayName,PrimaryEmail,PrimaryPhoneNumber,Gender,Company,CreatedDate) VALUES("
                    + "'"+UserId.ToString()  +"',"
                    +"'" + _personModel.FirstName + _personModel.LastName + "',"
                    + "'" + _personModel.FirstName + "',"
                    + "'" + _personModel.LastName + "',"
                    + "'" + _personModel.FirstName+ " " + _personModel.LastName + "',"
                    + "'" + _personModel.PrimaryEmail + "',"
                    + "'" + _personModel.PrimaryNumber + "',"
                    + "'" + _personModel.Gender + "',"
                    + "'" + _personModel.Country + "',"
                    + "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'"
                    + ")" ;

                SqlCommand command = new SqlCommand(insertUserQuery, _sqlConnection);
                command.ExecuteNonQuery();

                return true;
               
            }

         
            catch (Exception e)
            {


                string createUserTableQuery = @"
              CREATE TABLE SS_User (
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
)
           
        ";





                SqlCommand commandTable = new SqlCommand(createUserTableQuery, _sqlConnection);
                commandTable.ExecuteNonQuery();


                return false;
            }

     

        }







    }
}
