using SupportSystem.User.Model;
using SupportSystem.User.Tasks;

namespace SupportSystem.User.Roles;

public interface IAdminRoles : IBasicUserRoles, IManageCustomerAccount, ITicketEsclation, ITicketAssignment, IMeetingArrangement, ILogWork
{
    string GetUserRole(Guid UserId);

    bool SetUserRole(Guid UserId, string Role);

    bool CreateAgentAccount(IPersonModel person);




}