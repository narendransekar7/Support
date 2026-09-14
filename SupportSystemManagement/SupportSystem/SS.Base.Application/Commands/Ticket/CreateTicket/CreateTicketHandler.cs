using MassTransit;
using MediatR;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class CreateTicketHandler : IRequestHandler<CreateTicketCommand>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateTicketHandler(ITicketRepository ticketRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = new Ticket
            {
                TicketId = Guid.NewGuid(),
                Title = request.Title,
                // ResponseDueDate/ResolutionDueDate/AssignedTo are set later by the
                // ticket-creation saga (Assign Engineer -> Reserve SLA steps).
                CreatedBy = request.CreatedBy,
                Status = TicketStatus.Open,
                Priority = request.Priority,
                Visibility = request.Visibility
            };


            var ticketUpdate = new List<TicketUpdate>() {   new TicketUpdate
            {
                TicketId = ticket.TicketId,
                UpdatedBy = request.CreatedBy,
                Content = request.Content
            }};
            // Link First Ticket Update
            ticket.TicketUpdates = ticketUpdate;

            // Use the repository to add the ticket
            await _ticketRepository.AddAsync(ticket);

            // Commit changes using Unit of Work
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var creator = await _userRepository.GetByIdAsync(request.CreatedBy);

            // Kicks off the ticket-creation saga (Assign Engineer/Reserve SLA)
            // and the independent notify branch (Send Email/Create Notification).
            await _publishEndpoint.Publish(new TicketCreated
            {
                TicketId = ticket.TicketId,
                Title = ticket.Title,
                Priority = ticket.Priority,
                CreatedBy = ticket.CreatedBy,
                CreatedByEmail = creator?.PrimaryEmail,
                CreatedByName = creator?.DisplayName
            }, cancellationToken);

            return Unit.Value;
        }
    }
}
