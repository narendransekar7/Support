using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    // The refresh token is a random, unique value, so it identifies its user on its own: the SPA keeps it in an
    // HttpOnly cookie and never knows the user id.
    public class TokenRefreshCommand : IRequest<TokenRefreshResult>
    {
        public Guid RefreshToken { get; set; }
    }

    public class TokenRefreshResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public Guid NewRefreshToken { get; set; }
    }
}
