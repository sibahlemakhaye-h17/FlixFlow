using Microsoft.AspNetCore.Identity;

namespace FlixFlow.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsPremium { get; set; }

    // Navigation properties
    public ICollection<UserMovie>? UserMovies { get; set; }
}
