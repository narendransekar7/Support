using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.Base.Domain.Messages.User;

namespace SS.Base.Application.Commands
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<SS.Base.Domain.Entities.User> _passwordHasher;
        private readonly IEventPublisher _publisher;
        private readonly ILogger<CreateUserHandler> _logger;
        public CreateUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher<SS.Base.Domain.Entities.User> passwordHasher, IEventPublisher publisher, ILogger<CreateUserHandler> logger)
        {
            _logger = logger;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _publisher = publisher;
        }

        public async Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new SS.Base.Domain.Entities.User
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
                Password = _passwordHasher.HashPassword(user, request.Password)
            };

            // Link User to UserProfile
            user.Profile = userProfile;

            // Use the repository to add the user
            await _userRepository.AddAsync(user);

            // Commit changes using Unit of Work
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created user {UserId} with role {Role}", user.UserId, user.Role);
            
            // Welcome email, welcome notification and audit entry react to this independently, on whichever
            // broker Messaging:Provider selects. Keyed by user id (Kafka partition / Service Bus correlation).
            await _publisher.PublishAsync(new UserCreated
            {
                UserId = user.UserId,
                Email = user.PrimaryEmail,
                DisplayName = user.DisplayName,
                Role = user.Role.ToString()
            }, user.UserId.ToString(), cancellationToken);
            _logger.LogInformation("Published UserCreated for user {UserId}", user.UserId);

            return Unit.Value;
        }
    }
}