using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.Permission
{
    internal interface IUserPermission
    {
        Guid UserPermissionId { get; set; }
        Guid UserId { get; set; }
        Guid PermissionId { get; set; }
    }
}
