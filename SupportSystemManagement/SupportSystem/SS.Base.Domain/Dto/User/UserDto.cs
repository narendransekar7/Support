namespace SS.Base.Domain.Dto;
using SS.Base.Domain.Entities;
public class UserDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public Role Role { get; set; }
}