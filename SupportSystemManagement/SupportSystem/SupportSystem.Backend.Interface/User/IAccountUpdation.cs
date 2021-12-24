using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.Interface.User
{
    public interface IAccountUpdation
    {
        public Guid UserId { get; set; }
        bool UpdateFirstName(string FirstName);
        bool UpdateLastName(string LastName);
    }
}
