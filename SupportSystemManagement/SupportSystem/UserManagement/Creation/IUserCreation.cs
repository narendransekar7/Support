using SupportSystem.User.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Creation
{
    public interface IUserCreation
    {
        bool CreateUser(IPersonModel person);

    }
}
