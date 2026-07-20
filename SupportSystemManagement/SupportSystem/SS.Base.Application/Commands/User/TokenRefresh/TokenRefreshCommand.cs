using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class TokenRefreshCommand : IRequest
    {
        public Guid UserId { get; set; }
        public Guid RefreshToken { get; set; }
    }
}
