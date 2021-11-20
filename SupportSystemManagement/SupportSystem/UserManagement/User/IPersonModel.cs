using System;
using System.Collections.Generic;
using System.Text;
using TicketManagement;

namespace UserManagement.User
{
    public interface IPersonModel
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string EmailId { get; set; }
        string Country { get; set; }
        string Gender { get; set; }
    }
}
