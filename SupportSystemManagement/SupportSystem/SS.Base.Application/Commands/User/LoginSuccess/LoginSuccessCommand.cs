using MediatR;

namespace SS.Base.Application.Commands
{

    public class LoginSuccessCommand : IRequest
    {
        public Guid Token { get; set; }
        public string UserId { get; set; }
    }

}