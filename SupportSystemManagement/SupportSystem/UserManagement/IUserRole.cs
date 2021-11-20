using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Role
{
    public interface IUserRole
    {
        string GetUserRole(IPerson person);
    }

    public enum Role { }
}
