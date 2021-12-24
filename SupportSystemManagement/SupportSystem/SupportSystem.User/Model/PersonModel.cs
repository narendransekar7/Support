using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Model
{
    public class PersonModel : IPersonModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
       
        public string Country { get; set; }
        public string Gender { get; set; }
        public string PrimaryNumber { get; set; }
        public string PrimaryEmail { get; set; }
    }
}
