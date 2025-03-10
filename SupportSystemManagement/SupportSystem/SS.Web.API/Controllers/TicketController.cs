﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Commands;
using SS.Base.Application.Queries;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Dto;

namespace SS.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TicketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketCommand command)
        {
            await _mediator.Send(command);
            return Ok("Ticket created successfully");
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(Guid id)
        {
            var ticket = await _mediator.Send(new GetTicketByIdQuery(id));

            if (ticket == null)
                return NotFound();
            
            var ticketDto = new TicketDto
            {
                TicketId = ticket.TicketId,
                Title = ticket.Title,
                Status = ticket.Status.ToString(),
                Priority = ticket.Priority.ToString(),
                Visibility = ticket.Visibility.ToString(),
                CreatedBy = ticket.CreatedBy,
                AssignedTo = ticket.AssignedTo,
                Updates = ticket.TicketUpdates.Select(u => new TicketUpdateDto
                {
                    UpdateId = u.UpdateId,
                    Content = u.Content,
                    UpdatedAt = u.UpdatedAt,
                    UpdatedBy = u.UpdatedBy
                }).ToList()
            };

            return Ok(ticketDto);
        }
    }

}
