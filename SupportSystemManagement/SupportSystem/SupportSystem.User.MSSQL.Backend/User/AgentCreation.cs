namespace SupportSystem.User.MSSQL.Backend.User;

using SupportSystem.User.Backend.Interface.User;
using SupportSystem.User.Model;
using SupportSystem.User.MSSQL.Backend.Deployment;

public class AgentCreation : IAccountCreation
{
    private PersonModel _personModel;
    private UserDatabase _userDatabaseContext;
    
    
    public AgentCreation(UserDatabase userDatabaseContext,PersonModel personModel)
    {
        _userDatabaseContext = userDatabaseContext;
        _personModel = personModel;
    }

    public bool CreateAccount()
    {
        _personModel.UserId = Guid.NewGuid();
        try
        {
            _userDatabaseContext.SS_User.Add(_personModel); 
            _userDatabaseContext.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
    
}