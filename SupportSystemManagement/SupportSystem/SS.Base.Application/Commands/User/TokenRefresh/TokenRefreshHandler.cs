using MediatR;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands.User.TokenRefresh
{
    public class TokenRefreshHandler : IRequestHandler<TokenRefreshCommand, TokenRefreshResult>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TokenRefreshHandler(IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TokenRefreshResult> Handle(TokenRefreshCommand request, CancellationToken cancellationToken)
        {
            var existingToken = await _refreshTokenRepository.GetByIdAsync(request.RefreshToken);

            if (existingToken == null || existingToken.UserId != request.UserId.ToString())
            {
                return new TokenRefreshResult { Success = false, ErrorMessage = "Invalid refresh token." };
            }

            if (existingToken.IsRevoked)
            {
                return new TokenRefreshResult { Success = false, ErrorMessage = "Refresh token has been revoked." };
            }

            if (existingToken.IsUsed)
            {
                return new TokenRefreshResult { Success = false, ErrorMessage = "Refresh token has already been used." };
            }

            if (existingToken.ExpiryDate < DateTime.UtcNow)
            {
                existingToken.IsExpired = true;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new TokenRefreshResult { Success = false, ErrorMessage = "Refresh token has expired." };
            }

            var user = await _userRepository.GetByIdAsync(Guid.Parse(existingToken.UserId));
            if (user == null)
            {
                return new TokenRefreshResult { Success = false, ErrorMessage = "User not found." };
            }

            // Rotate: retire the old token and issue a new one
            existingToken.IsUsed = true;

            var newRefreshToken = new RefreshToken
            {
                Token = Guid.NewGuid(),
                UserId = existingToken.UserId,
                ExpiryDate = DateTime.Now.AddDays(7),
                IsRevoked = false
            };
            await _refreshTokenRepository.AddAsync(newRefreshToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TokenRefreshResult
            {
                Success = true,
                UserId = user.UserId.ToString(),
                Email = user.PrimaryEmail,
                Role = user.Role.ToString(),
                NewRefreshToken = newRefreshToken.Token
            };
        }
    }
}
