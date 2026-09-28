using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TokenRefreshHandler> _logger;

        public TokenRefreshHandler(IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<TokenRefreshHandler> logger)
        {
            _logger = logger;
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TokenRefreshResult> Handle(TokenRefreshCommand request, CancellationToken cancellationToken)
        {
            var existingToken = await _refreshTokenRepository.GetByIdAsync(request.RefreshToken);

            if (existingToken == null || existingToken.UserId != request.UserId.ToString())
            {
                _logger.LogWarning("Token refresh rejected for user {UserId}: invalid refresh token", request.UserId);
                return new TokenRefreshResult { Success = false, ErrorMessage = "Invalid refresh token." };
            }

            if (existingToken.IsRevoked)
            {
                _logger.LogWarning("Token refresh rejected for user {UserId}: token revoked", request.UserId);
                return new TokenRefreshResult { Success = false, ErrorMessage = "Refresh token has been revoked." };
            }

            if (existingToken.IsUsed)
            {
                // Reuse of an already-rotated token can mean a stolen token is being replayed.
                _logger.LogWarning("Token refresh rejected for user {UserId}: token already used (possible replay)", request.UserId);
                return new TokenRefreshResult { Success = false, ErrorMessage = "Refresh token has already been used." };
            }

            if (existingToken.ExpiryDate < DateTime.UtcNow)
            {
                existingToken.IsExpired = true;
                _logger.LogInformation("Token refresh rejected for user {UserId}: token expired", request.UserId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new TokenRefreshResult { Success = false, ErrorMessage = "Refresh token has expired." };
            }

            var user = await _userRepository.GetByIdAsync(Guid.Parse(existingToken.UserId));
            if (user == null)
            {
                _logger.LogWarning("Token refresh rejected: user {UserId} not found", existingToken.UserId);
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
            _logger.LogInformation("Rotated refresh token for user {UserId}", user.UserId);

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
