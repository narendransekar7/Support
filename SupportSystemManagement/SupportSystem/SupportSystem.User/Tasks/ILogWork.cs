using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.User.Tasks
{
    public interface ILogWork
    {
        bool AddWorkLog(Guid UserId, int Minutes);

    }
}
