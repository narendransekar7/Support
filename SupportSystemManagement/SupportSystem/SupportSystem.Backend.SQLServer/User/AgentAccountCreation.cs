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

                return false;
            }

     

        }

    }
}
