using System.ComponentModel.DataAnnotations;

namespace SupportSystem.User.Model;

public class PersonModel : IPersonModel
{
    [Key]
    public Guid UserId { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    public string DisplayName { get; set; }

    public string Country { get; set; }
    public string Gender { get; set; }
    public string PrimaryNumber { get; set; }
    [Required]
    public string PrimaryEmail { get; set; }
    [Required]
    public string Password { get; set; }
    
}