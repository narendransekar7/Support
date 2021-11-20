using System;
using System.Collections.Generic;
using System.Text;

namespace SupportSystem.Backend.SQLServer.Connection
{
   public class ParameterModel: IParameterModel
    {
       public string Server { get; set; }
       public string Database { get; set; }

       public  string User_Id { get; set; }

       public string Password { get; set; }

    }
}
