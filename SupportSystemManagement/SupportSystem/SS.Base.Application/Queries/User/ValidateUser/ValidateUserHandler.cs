using MediatR;
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
    public class ValidateUserHandler: IRequestHandler<ValidateUserQuery,bool>
    {

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        
        public ValidateUserHandler(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> Handle(ValidateUserQuery request, CancellationToken cancellationToken)
        {
            User user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null) return false;
            if (user.Profile.Password==request.Password)
            //if (_passwordHasher.VerifyHashedPassword(user, user.Profile.Password, request.Password) == PasswordVerificationResult.Success)
                return true;
            else
                return false;
        }

    }
}
