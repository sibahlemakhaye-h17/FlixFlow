using System.ComponentModel.DataAnnotations;

namespace FlixFlow.ViewModels;

public class ProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}".Trim();

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Member Since")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Premium Member")]
    public bool IsPremium { get; set; }

    public string? ProfilePictureUrl { get; set; }

    // Stats
    public int WatchlistCount { get; set; }
    public int FavoritesCount { get; set; }
    public int RatingsCount { get; set; }
}
