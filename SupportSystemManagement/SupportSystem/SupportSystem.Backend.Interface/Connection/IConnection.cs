using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.Interface.Connection
{
    public interface IConnection<Connection>
    {
        Connection CreateConnection();
    }
}
