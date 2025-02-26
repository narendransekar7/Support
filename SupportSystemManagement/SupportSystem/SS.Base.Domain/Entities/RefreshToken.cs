using System.ComponentModel.DataAnnotations;

namespace SS.Base.Domain.Entities;

public class RefreshToken
{
    [Key]
    public Guid Token { get; set; }
    [Required]
    public string UserId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
}