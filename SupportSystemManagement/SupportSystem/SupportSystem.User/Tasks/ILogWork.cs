namespace SupportSystem.User.Tasks;

public interface ILogWork
{
    bool AddWorkLog(Guid UserId, int Minutes);

}