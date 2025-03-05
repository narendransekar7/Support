namespace SS.Base.Domain.Email;

public class UserCreatedMessage
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
}