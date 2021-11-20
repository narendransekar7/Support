using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.SQLServer.Connection
{
   public interface IParameterModel
    {
        string Server { get; set; }
        string Database { get; set; }

        string User_Id { get; set; }

        string Password { get; set; }

    }
}
