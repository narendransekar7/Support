using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LogOutHandler> _logger;

        public LogOutHandler(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, ILogger<LogOutHandler> logger)
        {
            _logger = logger;
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
                _logger.LogInformation("User {UserId} logged out; refresh token revoked", refreshTokens.UserId);
            }
            else
            {
                _logger.LogWarning("Logout: refresh token not found, nothing to revoke");
            }
            // Need to check whether it works after clik on the logout option from the react

            // Commit changes using Unit of Work
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
