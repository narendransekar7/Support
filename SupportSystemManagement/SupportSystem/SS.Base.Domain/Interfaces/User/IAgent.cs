using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.User
{
    public interface IAgent : IUser
    {
        bool IsAdmin { get; set; }
        bool IsSales { get; set; }

        bool IsInternal { get; set; }

        bool IsCoordinator { get; set; }
    }
}
