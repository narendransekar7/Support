﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SupportManagement.Ticket.Model.Field
{

    public interface ITicketField<T>
    {
        Guid FieldId { get; set; }

        TicketFilesTypes Type { get; set; }

        T Value { get; set; }

    }


    public enum TicketFilesTypes
    {
        Combobox,
        ListBox,
        TextBox,
        Checkbox
    }

}
