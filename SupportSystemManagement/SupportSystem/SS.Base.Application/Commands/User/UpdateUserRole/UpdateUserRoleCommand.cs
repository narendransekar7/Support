using SS.Base.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class UpdateUserRoleCommand
    {
        public Guid UserId { get; set; }
        public Role Role { get; set; }
    }
}
