namespace SupportSystem.User.Model;

public interface IAgentModel : IPersonModel
{
    bool IsAdmin { get; set; }
    bool IsSales { get; set; }

    bool IsInternal { get; set; }

    bool IsCoordinator { get; set; }
}