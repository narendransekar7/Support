using SupportSystem.User.Tasks;

namespace SupportSystem.User.Roles;

public interface ICustomerRoles : IBasicUserRoles, ITicketEsclation, ITicketAssignment
{


}