namespace SupportSystem.User.Backend.Interface.Connection;

public interface IConnection<Connection>
{
    Connection CreateConnection();
}