using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.Interface.User
{
    public interface ICreateUserQuery
    {
        bool ExecuteUserAddQuery(IPersonModel person);
    }
}
