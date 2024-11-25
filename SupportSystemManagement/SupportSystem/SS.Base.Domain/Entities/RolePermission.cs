
using SS.Base.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Entities
{
    public class RolePermission
    {
        public Guid RolePermissionId { get; set; }
        public Guid PermissionId { get; set; }
        public Role Role { get; set; }
    }
}
