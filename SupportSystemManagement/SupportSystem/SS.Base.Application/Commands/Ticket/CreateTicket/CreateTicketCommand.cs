using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class CreateTicketCommand : IRequest
    {
        public string Title { get; set; }
        public string Priority { get; set; }
        public string Visibility { get; set; }
        public Guid CreatedBy { get; set; }

        public Guid AssignedTo { get; set; }

        public string Content { get; set; }
    }
}
