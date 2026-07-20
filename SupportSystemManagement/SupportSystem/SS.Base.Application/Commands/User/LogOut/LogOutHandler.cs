using MediatR;
using Microsoft.Azure.Amqp.Framing;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands.User.LogOut
{
    public class LogOutHandler: IRequestHandler<LogOutCommand>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LogOutHandler(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(LogOutCommand request, CancellationToken cancellationToken)
        {
            // Fetch the refresh tokens associated with the user
            var refreshTokens = await _refreshTokenRepository.GetByIdAsync(request.RefreshToken);

            // Update IsRevoked and IsExpired
            if (refreshTokens != null) {
                refreshTokens.IsRevoked = true;
                //refreshTokens.IsExpired = true;
            }
            // Need to check whether it works after clik on the logout option from the react

            // Commit changes using Unit of Work
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
