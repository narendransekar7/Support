using System.ComponentModel.DataAnnotations;

namespace SS.Base.Domain.Entities;

/// <summary>
/// Single-row table tracking the last Agent assigned to a ticket, so engineer
/// assignment can cycle through Role.Agent users in round-robin order.
/// </summary>
public class RoundRobinCursor
{
    [Key]
    public int Id { get; set; }
    public Guid? LastAssignedUserId { get; set; }
}
