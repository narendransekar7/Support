using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Entities
{
    public class UserProfile
    {
        [Key]
        public Guid UserId { get; set; }
        [Required]
        public string Password { get; set; }
        public string? Country { get; set; }
        public string? Gender { get; set; }
        public string? PrimaryNumber { get; set; }
        [Required]
        // Navigation done through data annotation here. but we can done through Fluent API
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
