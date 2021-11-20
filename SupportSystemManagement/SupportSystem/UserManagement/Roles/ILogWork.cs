using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Roles
{
   public interface ILogWork
    {
        bool AddWorkLog(Guid UserId, int Minutes);

    }
}
