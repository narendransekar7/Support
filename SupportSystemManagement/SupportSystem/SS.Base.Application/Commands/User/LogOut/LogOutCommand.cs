using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands.User.LogOut
{
    // Identified by the refresh token alone (see TokenRefreshCommand).
    public class LogOutCommand: IRequest
    {
        public Guid RefreshToken { get; set; }
    }
}
