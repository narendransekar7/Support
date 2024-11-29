using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Entities
{
    public class UserPermission
    {
        public Guid UserPermissionId { get; set; }
        public Guid UserId { get; set; }
        public Guid PermissionId { get; set; }
    }
}
