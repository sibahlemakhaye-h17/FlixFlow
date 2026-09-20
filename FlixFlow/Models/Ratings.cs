using System.ComponentModel.DataAnnotations;

namespace FlixFlow.Models;

public class Rating
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int MovieId { get; set; }

    [Range(1, 10)]
    public int Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
