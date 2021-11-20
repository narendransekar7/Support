using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement
{
    public interface IPerson
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string EmailId { get; set; }
        string Country { get; set; }
        string Gender { get; set; }

        bool CreateAccount(IPerson person);

        bool AssignRole(IPerson person);

        bool UpdateUserDetail(IPerson person);
    }
}
