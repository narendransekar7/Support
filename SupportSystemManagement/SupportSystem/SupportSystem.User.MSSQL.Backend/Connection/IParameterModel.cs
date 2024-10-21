namespace SupportSystem.User.MSSQL.Backend.Connection;

public interface IParameterModel
{
    string Server { get; set; }
    string Database { get; set; }

    string User_Id { get; set; }

    string Password { get; set; }

}

