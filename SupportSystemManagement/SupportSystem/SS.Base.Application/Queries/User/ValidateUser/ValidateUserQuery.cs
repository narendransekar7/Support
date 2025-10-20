using MediatR;
using SS.Base.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class ValidateUserQuery:IRequest<SS.Base.Domain.Entities.User?>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
