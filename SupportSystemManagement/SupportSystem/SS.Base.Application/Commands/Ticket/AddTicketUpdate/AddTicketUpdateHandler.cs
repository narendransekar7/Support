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
    public class AddTicketUpdateHandler : IRequestHandler<AddTicketUpdateCommand>
    {
        private readonly ITicketUpdateRepository _ticketUpdateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddTicketUpdateHandler(ITicketUpdateRepository ticketUpdateRepository, IUnitOfWork unitOfWork)
        {
            _ticketUpdateRepository = ticketUpdateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(AddTicketUpdateCommand request, CancellationToken cancellationToken)
        {
            var ticketUpdate = new TicketUpdate
            {
                TicketId = request.TicketId,
                UpdatedBy = request.UpdatedBy,
                Content = request.Content
            };

            // Use the repository to add the ticketupdate
            await _ticketUpdateRepository.AddAsync(ticketUpdate);

            // Commit changes using Unit of Work
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
