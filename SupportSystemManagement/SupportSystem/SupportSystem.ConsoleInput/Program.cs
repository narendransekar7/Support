using SupportSystem.User.Creation;
using SupportSystem.User.Factory;
using SupportSystem.User.Model;
using System;

namespace SupportSystem.ConsoleInput
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welome to Support System");
            Console.WriteLine("Enter the options");
            Console.WriteLine("Create User");
            Console.WriteLine("Create Product");
            Console.WriteLine("Create Ticket");




            string input = Console.ReadLine();
            switch (Convert.ToInt32(input))
            {

                case 1:
                    Console.WriteLine("Press 1 to create Customer User");
                    Console.WriteLine("Press 2 to create Agent User");
                    IUserCreation User;
                    IPersonModel person = UserFactory.GetPersonModel();
                    person.FirstName = Console.ReadLine();
                    person.LastName = Console.ReadLine();
                    person.Country = Console.ReadLine();
                    person.EmailId = Console.ReadLine();
                    if (Convert.ToInt32(Console.ReadLine()) == 1)
                    {
                        User = UserFactory.CreateCustomer();
                    }
                    else
                    {
                        User = UserFactory.CreateAgent();
                    }


                    User.CreateUser(person);

                    break;

                case 2:

                    break;



            }






            Console.ReadLine();
        }
    }
}
