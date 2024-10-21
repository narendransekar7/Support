using SupportSystem.User.Backend.Interface.User;
using SupportSystem.User.Model;

namespace SupportSystem.User.MSSQL.Backend.User;

public class CustomerAccountCreation : IAccountCreation
{

    public CustomerAccountCreation(IPersonModel person)
    {


    }


    public bool CreateAccount()
    {
        throw new NotImplementedException();
    }
}