using MediatR;
using Microsoft.AspNetCore.Identity;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.Base.Application.Events;
using SS.Base.Domain.Email;

namespace SS.Base.Application.Commands
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher;
        private AzureServiceBusQueueSender _queueSender;
        public CreateUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, AzureServiceBusQueueSender queueSender)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _queueSender = queueSender;
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
               // Password = _passwordHasher.HashPassword(user, request.Password)
            };

            // Link User to UserProfile
            user.Profile = userProfile;

            // Use the repository to add the user
            await _userRepository.AddAsync(user);

            // Commit changes using Unit of Work
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Send message to Azure Service Bus Queue
            var userCreatedMessage = new UserCreatedMessage
            {
                UserId = user.UserId,
                Email = user.PrimaryEmail,
                FullName = user.DisplayName
            };
            
            await _queueSender.SendMessageAsync(userCreatedMessage);
            return Unit.Value;
        }
    }
}