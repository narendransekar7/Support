using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.User
{
    public interface IUserProfile
    {
        [Required]
        public string Password { get; set; }
        public string Country { get; set; }
        public string PrimaryNumber { get; set; }
        public string Gender { get; set; }
        
    }
}
