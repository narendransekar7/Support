using SS.Base.Domain.Interfaces.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Entities.User
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        [Required]
        public string PrimaryEmail { get; set; }
        [Required]
        public string Role { get; set; }

        public virtual UserProfile Profile { get; set; }
    }
}
