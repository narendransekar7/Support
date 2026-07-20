using MediatR;
using Microsoft.AspNetCore.Identity;
using SS.Base.Application.Commands.User.LogOut;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands.User.TokenRefresh
{
    public class TokenRefreshHandler : IRequestHandler<TokenRefreshCommand>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TokenRefreshHandler(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(TokenRefreshCommand request, CancellationToken cancellationToken)
        {
            // Fetch the refresh tokens associated with the user
            var refreshTokens = await _refreshTokenRepository.GetByIdAsync(request.RefreshToken);

            // Need to check the below highliged changes not sure.
            //var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenDto.RefreshToken);

            //if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            //{
            //    return Unauthorized(new { message = "Invalid or expired refresh token" });
            //}

            //var roles = await _userManager.GetRolesAsync(user);
            //var (newAccessToken, newRefreshToken) = _tokenService.GenerateTokens(user, roles);

            //user.RefreshToken = newRefreshToken;
            //user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            //await _userManager.UpdateAsync(user);






            if (refreshTokens != null)
            {
                // Update IsRevoked and IsExpired
                if (refreshTokens.ExpiryDate < DateTime.UtcNow)
                {
                    refreshTokens.IsUsed = true;
                    
                }
                else if(refreshTokens.ExpiryDate > DateTime.UtcNow)
                {
                    refreshTokens.IsExpired = true;
                }
            }
            // Commit changes using Unit of Work
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
