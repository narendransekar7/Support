using System;
using System.Collections.Generic;
using System.Text;
using TicketManagement;

namespace SupportSystem.User.Model
{
    public interface IPersonModel
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string Country { get; set; }

        string PrimaryNumber { get; set; }
        string Gender { get; set; }

        string PrimaryEmail { get; set; }
    }
}
