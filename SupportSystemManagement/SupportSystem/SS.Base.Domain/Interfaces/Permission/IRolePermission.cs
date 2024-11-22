using SS.Base.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.Permission
{
    public interface IRolePermission
    {
        Guid RolePermissionId { get; set; }
        Guid PermissionId { get; set; }
        Role Role { get; set; }
    }
}
