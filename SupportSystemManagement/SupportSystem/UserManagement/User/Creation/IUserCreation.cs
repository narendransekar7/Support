using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.User.Creation
{
   public interface IUserCreation
    {
         bool CreateUser(IPersonModel person);
       
    }
}
