using MediatR;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                DisplayName = request.FirstName +' '+ request.LastName ,
                PrimaryEmail = request.PrimaryEmail,
            };

            var userProfile = new UserProfile
            {
                UserId = user.UserId,
                Password = request.Password
            };

            // Link User to UserProfile
            user.Profile = userProfile;

            // Use the repository to add the user
            await _userRepository.AddAsync(user);

            // Commit changes using Unit of Work
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
