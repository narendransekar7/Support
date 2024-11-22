using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.Permission
{
    public interface IPermission
    {
        Guid PermissionId { get; set; }
        string Name { get; set; }
        string Description { get; set; }   
        DateTime CreatedAt { get; set; }
    }
}
