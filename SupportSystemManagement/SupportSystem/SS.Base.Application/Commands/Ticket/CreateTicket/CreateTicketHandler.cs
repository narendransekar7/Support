using MediatR;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
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
        private readonly IUnitOfWork _unitOfWork;

        public CreateTicketHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = new Ticket
            {
                TicketId = Guid.NewGuid(),
                Title = request.Title,
                ResolutionDueDate = DateTime.Now.AddDays(7),
                ResponseDueDate = DateTime.Now.AddDays(1),
                CreatedBy = request.CreatedBy,
                Status = TicketStatus.Open,
                Priority = request.Priority,
                AssignedTo = request.AssignedTo,
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

            return Unit.Value;
        }
    }
}
