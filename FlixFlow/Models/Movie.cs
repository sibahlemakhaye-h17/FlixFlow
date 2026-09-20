using System.ComponentModel.DataAnnotations;

namespace FlixFlow.Models;

public class Movie
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Release Date")]
    public DateTime? ReleaseDate { get; set; }

    [Display(Name = "Poster")]
    public string? PosterPath { get; set; }

    public string? BackdropPath { get; set; }

    [Display(Name = "Rating")]
    public double? VoteAverage { get; set; }

    public int? VoteCount { get; set; }

    public int? Runtime { get; set; }

    [Display(Name = "Language")]
    public string? OriginalLanguage { get; set; }

    public string? TrailerUrl { get; set; }

    // TMDB reference
    public int TmdbId { get; set; }

    // Navigation properties
    public ICollection<UserMovie>? UserMovies { get; set; }
}

public class UserMovie
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int MovieId { get; set; }
    public bool IsWatchlist { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime AddedDate { get; set; }

    // Navigation properties
    public Movie Movie { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
