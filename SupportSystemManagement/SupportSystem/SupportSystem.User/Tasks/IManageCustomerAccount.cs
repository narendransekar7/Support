using SupportSystem.User.Model;

namespace SupportSystem.User.Tasks;

public interface IManageCustomerAccount
{
    bool CreateCustomerAccount(ICustomerModel person);

    bool UpdateCustomerAccount(ICustomerModel person);
}