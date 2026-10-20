using MediatR;
using Microsoft.Extensions.Logging;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Messaging;
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
        private readonly IEventPublisher _publisher;
        private readonly ILogger<CreateTicketHandler> _logger;

        public CreateTicketHandler(ITicketRepository ticketRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IEventPublisher publisher, ILogger<CreateTicketHandler> logger)
        {
            _logger = logger;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
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
            _logger.LogInformation("Created ticket {TicketId} ({Priority}) for user {UserId}", ticket.TicketId, ticket.Priority, ticket.CreatedBy);

            var creator = await _userRepository.GetByIdAsync(request.CreatedBy);

            // Kicks off the ticket-creation saga (Assign Engineer/Reserve SLA) and the independent
            // email/notification/audit consumers, on whichever broker Messaging:Provider selects.
            // Keyed by ticket id so all of this ticket's messages share a Kafka partition (ordering).
            await _publisher.PublishAsync(new TicketCreated
            {
                TicketId = ticket.TicketId,
                Title = ticket.Title,
                Priority = ticket.Priority,
                CreatedBy = ticket.CreatedBy,
                CreatedByEmail = creator?.PrimaryEmail,
                CreatedByName = creator?.DisplayName
            }, ticket.TicketId.ToString(), cancellationToken);
            _logger.LogInformation("Published TicketCreated for ticket {TicketId}", ticket.TicketId);

            return Unit.Value;
        }
    }
}
