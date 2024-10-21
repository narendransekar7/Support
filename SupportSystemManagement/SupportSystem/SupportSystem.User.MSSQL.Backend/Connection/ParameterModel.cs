namespace SupportSystem.User.MSSQL.Backend.Connection;

public class ParameterModel: IParameterModel
{
    public string Server { get; set; }
    public string Database { get; set; }

    public  string User_Id { get; set; }

    public string Password { get; set; }

}