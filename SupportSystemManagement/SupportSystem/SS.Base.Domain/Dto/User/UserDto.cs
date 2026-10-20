namespace SS.Base.Domain.Dto;
using SS.Base.Domain.Entities;
public class UserDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    // Filled only where the caller may see other accounts' addresses (the admin user list).
    public string? Email { get; set; }
    public Role Role { get; set; }
}