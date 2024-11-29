﻿using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Application.Commands
{
    public class ValidateUserQuery:IRequest<bool>
    {
        public string PrimaryEmail { get; set; }
        public string Password { get; set; }
    }
}