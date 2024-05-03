using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportSystem.WebAPI.Model.MetaData
{
    public class ServerDetail
    {
        public string Server { get; set; }

        public DatabaseType Type;

        public string User_Id { get; set; }

        public string Password { get; set; }

        public string Database { get; set; }
    }
}
