namespace SupportSystem.Ticket.Model;

using SupportSystem.Ticket.Updation;

public interface ITicketModel
{

    /// <summary>
    /// Id of the user who creates the ticket.
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Title of the ticket
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Ticket Created date.
    /// </summary>
    DateTime CreatedDate { get; set; }

    /// <summary>
    /// Next Action date.
    /// </summary>
    DateTime ResolutionDate { get; set; }

    /// <summary>
    /// Rating provided for the ticket which lies between 1 to 5.
    /// </summary>
    int Rating { get; set; }

    /// <summary>
    /// Feedback provided by the user
    /// </summary>
    string FeedbackComment { get; set; }

    /// <summary>
    /// Indicates the user who created the ticket.
    /// </summary>
    Guid CreatorId { get; set; }

    /// <summary>
    /// Indicates the current status of the ticket.
    /// </summary>
    string Status { get; set; }

    /// <summary>
    /// List of usersid watching the ticket.
    /// </summary>
    List<Guid> Wachers { get; set; }

    /// <summary>
    /// Can be assigned to User or Group.
    /// </summary>
    IAssignee Assignee { get; set; }

    /// <summary>
    /// Ticket can be public, private or internal.
    /// </summary>
    string Visibility { get; set; }

}