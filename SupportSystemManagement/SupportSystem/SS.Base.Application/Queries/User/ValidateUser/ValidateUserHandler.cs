using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using SS.Base.Application.Commands;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Queries
{
    public class ValidateUserHandler : IRequestHandler<ValidateUserQuery, User?>
    {

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<ValidateUserHandler> _logger;
        
        public ValidateUserHandler(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, ILogger<ValidateUserHandler> logger)
        {
            _logger = logger;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> Handle(ValidateUserQuery request, CancellationToken cancellationToken)
        {
            User user = await _userRepository.ValidateUserByCredentialAsync(request.Email);
            if (user == null)
            {
                // Email deliberately not logged (PII); failed logins are still countable/alertable by this message.
                _logger.LogWarning("Login failed: unknown user");
                return null;
            }
            if (user.Profile.Password==request.Password)
            //if (_passwordHasher.VerifyHashedPassword(user, user.Profile.Password, request.Password) == PasswordVerificationResult.Success)
                return user;
            else
            {
                _logger.LogWarning("Login failed: wrong password for user {UserId}", user.UserId);
                return null;
            }
        }

    }
}
