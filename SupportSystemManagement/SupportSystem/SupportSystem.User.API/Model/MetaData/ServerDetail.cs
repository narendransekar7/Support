namespace SupportSystem.User.API.Model.MetaData;

public class ServerDetail
{
    public string Server { get; set; }

    public DatabaseType Type;

    public string User_Id { get; set; }

    public string Password { get; set; }

    public string Database { get; set; }
}