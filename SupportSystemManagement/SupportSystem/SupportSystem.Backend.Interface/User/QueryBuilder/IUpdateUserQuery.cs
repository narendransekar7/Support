using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.Interface.User.QueryBuilder
{
    public interface IUpdateUserQuery
    {

        bool UpdateUserFirstName(Guid UserId, string FirstName);
        bool UpdateUserLastName(Guid UserId, string LastName);

    }
}
