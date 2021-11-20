using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.User;

namespace SupportSystem.Backend.Interface.User
{
    public interface ICreateUserQuery
    {
        bool ExecuteUserAddQuery(IPersonModel person);
    }
}
