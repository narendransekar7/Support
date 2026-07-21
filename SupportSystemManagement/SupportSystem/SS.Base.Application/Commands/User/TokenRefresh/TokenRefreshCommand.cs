using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class TokenRefreshCommand : IRequest<TokenRefreshResult>
    {
        public Guid UserId { get; set; }
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
