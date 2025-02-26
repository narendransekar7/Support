using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.Base.Domain.Entities;

namespace SS.Base.Application.Commands
{
    public class CreateUserCommand : IRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public string Country { get; set; }
        
        public string Gender { get; set; }
        
        public string PrimaryEmail { get; set; }
        public string PrimaryNumber { get; set; }
        
        public string Password { get; set; }
        public Role Role { get; set; }
        
    }
}
