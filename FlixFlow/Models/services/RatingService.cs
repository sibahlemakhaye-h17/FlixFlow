using Microsoft.EntityFrameworkCore;
using FlixFlow.Data;
using FlixFlow.Interfaces;
using FlixFlow.Models;

namespace FlixFlow.Services;

public class RatingService : IRatingService
{
    private readonly ApplicationDbContext _context;
    private readonly ITMDBService _tmdbService;
    private readonly ILogger<RatingService> _logger;

    public RatingService(
        ApplicationDbContext context,
        ITMDBService tmdbService,
        ILogger<RatingService> logger)
    {
        _context = context;
        _tmdbService = tmdbService;
        _logger = logger;
    }

    public async Task<int?> GetUserRatingAsync(string userId, int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null) return null;

        var rating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movie.Id);

        return rating?.Score;
    }

    public async Task<double?> GetAverageRatingAsync(int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null) return null;

        var ratings = await _context.Ratings
            .Where(r => r.MovieId == movie.Id)
            .ToListAsync();

        if (!ratings.Any()) return null;

        return ratings.Average(r => r.Score);
    }

    public async Task<int> GetRatingCountAsync(int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null) return 0;

        return await _context.Ratings.CountAsync(r => r.MovieId == movie.Id);
    }

    public async Task<bool> RateMovieAsync(string userId, int tmdbId, int score)
    {
        if (score < 1 || score > 10) return false;

        var movie = await GetOrCreateMovieAsync(tmdbId);
        if (movie == null) return false;

        var existing = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movie.Id);

        if (existing != null)
        {
            existing.Score = score;
            existing.UpdatedAt = DateTime.Now;
        }
        else
        {
            _context.Ratings.Add(new Rating
            {
                UserId = userId,
                MovieId = movie.Id,
                Score = score,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("User {UserId} rated movie {TmdbId}: {Score}", userId, tmdbId, score);
        return true;
    }

    public async Task<bool> RemoveRatingAsync(string userId, int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null) return false;

        var rating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movie.Id);

        if (rating == null) return false;

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUserRatingsCountAsync(string userId)
    {
        return await _context.Ratings.CountAsync(r => r.UserId == userId);
    }

    private async Task<Movie?> GetOrCreateMovieAsync(int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie != null) return movie;

        var movieFromTmdb = await _tmdbService.GetMovieDetailsAsync(tmdbId);
        if (movieFromTmdb == null) return null;

        _context.Movies.Add(movieFromTmdb);
        await _context.SaveChangesAsync();
        return movieFromTmdb;
    }
}