using System.ComponentModel.DataAnnotations;

namespace SupportSystem.User.Model;

public interface IPersonModel
{
    Guid UserId { get; set; }
    string FirstName { get; set; }
    
    string LastName { get; set; }

    string DisplayName { get; set; }
    string Country { get; set; }

    string PrimaryNumber { get; set; }
    string Gender { get; set; }
    
    string PrimaryEmail { get; set; }
    string Password { get; set; }
}