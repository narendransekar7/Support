using SupportSystem.User.Backend.Interface.User;

namespace SupportSystem.User.MSSQL.Backend.User;

public class AgentAccountUpdation : IAccountUpdation
{

    public AgentAccountUpdation()
    {

    }

    public Guid UserId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool UpdateFirstName(string FirstName)
    {
        throw new NotImplementedException();
    }

    public bool UpdateLastName(string LastName)
    {
        throw new NotImplementedException();
    }
}