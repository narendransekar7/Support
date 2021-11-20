using System;
using System.Collections.Generic;
using System.Text;

namespace TicketManagement.Updation.Model
{
    public interface ITicketUpdateModel
    {
    
        Guid PublishId { get; set; }

        Guid TicketId { get; set; }

        Guid UserId { get; set; }

        DateTime UpdatedDate { get; set; }

        string OldValue { get; set; }

        string NewValue { get; set; }

    }
}
