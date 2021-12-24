using SupportSystem.Backend.Interface.Connection;
using SupportSystem.Backend.Interface.User;
using SupportSystem.Backend.SQLServer.Connection;
using SupportSystem.Backend.SQLServer.Factory;
using SupportSystem.User.Creation;
using SupportSystem.User.Factory;
using SupportSystem.User.Model;
using System;
using System.Data.SqlClient;

namespace SupportSystem.ConsoleInput
{
    class Program
    {
        static void Main(string[] args)
        {

            #region Support System Options

            Console.WriteLine("\t\t\t\t\t\tWelome to Support System");
            Console.WriteLine("Enter the options");
            Console.WriteLine("1. Create User");
            Console.WriteLine("2. Create Ticket");
            Console.WriteLine("3. Create Product");
            string input = Console.ReadLine();

            //Consider it as stored in some JSON or XML in the application.
            string Server = ".";
            string Database = "SupportSystem";
            string User_Id = "SupportAdmin";
            string Passowrd = "NarenSql@123";

            //Common parameter creation.
            IParameterModel Parameters = new ParameterModel();
            Parameters.Server = Server;
            Parameters.User_Id = User_Id;
            Parameters.Password = Passowrd;
            Parameters.Database = Database;

            #endregion

            switch (Convert.ToInt32(input))
            {

                case 1:
                    IPersonModel person = UserFactory.GetPersonModel();
                    Console.WriteLine("Enter the First Name");
                    person.FirstName = Console.ReadLine();
                    Console.WriteLine("Enter the Last Name");
                    person.LastName = Console.ReadLine();
                    Console.WriteLine("Enter the Country");
                    person.Country = Console.ReadLine();
                    Console.WriteLine("Enter the Email");
                    person.PrimaryEmail = Console.ReadLine();
                    Console.WriteLine("Enter the Gender");
                    person.Gender = Console.ReadLine();
                    Console.WriteLine("Enter the Phone Number");
                    person.PrimaryNumber = Console.ReadLine();



                    Console.WriteLine("Select the BackEnd Type");
                    Console.WriteLine("1. SQL Server");
                    Console.WriteLine("2. Oracle");


                    #region Backend Selection and connection creation

                    //IAccountCreation BackEndAccount;
                    if (Convert.ToInt32(Console.ReadLine()) == 1)
                    {
                        #region User Account Selection

                        Console.WriteLine("Select the User Type");
                        Console.WriteLine("1. Agent");
                        Console.WriteLine("2. Customer");

                        //IUserCreation userCreation;

                        IAccountCreation accountCreation;

                        if (Convert.ToInt32(Console.ReadLine()) == 1)
                        {

                            SqlConnection sqlConnection = SQLSeverBackendFactory.CreateSQLServerConnection(Parameters);


                            accountCreation = SQLSeverBackendFactory.CreateAgentAccount(sqlConnection,person);

                            UserFactory.CreateAgent(accountCreation);


                           // userCreation = new AgentCreation(accountCreation);
                           //userCreation.CreateUser(person);


                            //Working code
                            //accountCreation = SQLSeverBackendFactory.CreateAgentAccount(SQLSeverBackendFactory.CreateSQLServerConnection(Parameters),person);
                            //accountCreation.CreateAccount();
                            //UserFactory.CreateAgent(person, accountCreation).CreateUser();
                        }
                        else
                        {
                           // UserFactory.CreateCustomer(person, Connection).CreateUser();
                        }


                        #endregion
                    }
                    else
                    {
                        // Oracle code

                    }

                    #endregion





                    break;

                case 2:

                    break;



            }






            Console.ReadLine();
        }
    }
}
