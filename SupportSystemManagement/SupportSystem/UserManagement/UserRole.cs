using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Role;

namespace UserManagement
{
    public class UserRole: IUserRole
    {
        public string GetUserRole(IPerson person)
        {

          return  Role.Admin.ToString();

        }

        public enum Role{
        
            Admin = 1,
            InternalTeam = 2,
            Sales = 3,
            Customer = 4,
            SuperCustomer = 5   
        }




    }
}
