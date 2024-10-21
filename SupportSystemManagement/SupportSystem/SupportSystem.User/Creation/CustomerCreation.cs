using SupportSystem.User.Model;

namespace SupportSystem.User.Creation;

public class CustomerCreation : IUserCreation
{


    dynamic _connection;
    IPersonModel _person;

    public CustomerCreation(IPersonModel person, dynamic connection)
    {
        _person = person;
        _connection = connection;
    }


    public bool CreateUser()
    {

        return true;
    }
}