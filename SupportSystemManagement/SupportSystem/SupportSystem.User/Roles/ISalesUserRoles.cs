using SupportSystem.User.Tasks;

namespace SupportSystem.User.Roles;

public interface ISalesUserRoles : IBasicUserRoles, IManageCustomerAccount, IManageInternalNotes, ITicketEsclation, ILogWork
{

}