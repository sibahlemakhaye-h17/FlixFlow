using Microsoft.EntityFrameworkCore;
using FlixFlow.Data;
using FlixFlow.Interfaces;
using FlixFlow.Models;

namespace FlixFlow.Services;

public class WatchlistService : IWatchlistService
{
    private readonly ApplicationDbContext _context;
    private readonly ITMDBService _tmdbService;
    private readonly ILogger<WatchlistService> _logger;

    public WatchlistService(
        ApplicationDbContext context,
        ITMDBService tmdbService,
        ILogger<WatchlistService> logger)
    {
        _context = context;
        _tmdbService = tmdbService;
        _logger = logger;
    }

    public async Task<List<Movie>> GetWatchlistAsync(string userId)
    {
        return await _context.UserMovies
            .Include(um => um.Movie)
            .Where(um => um.UserId == userId && um.IsWatchlist)
            .OrderByDescending(um => um.AddedDate)
            .Select(um => um.Movie)
            .ToListAsync();
    }

    public async Task<bool> AddToWatchlistAsync(string userId, int tmdbId)
    {
        var movie = await GetOrCreateMovieAsync(tmdbId);
        if (movie == null) return false;

        var existing = await _context.UserMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movie.Id);

        if (existing != null)
        {
            if (existing.IsWatchlist) return true;

            existing.IsWatchlist = true;
            await _context.SaveChangesAsync();
            return true;
        }

        _context.UserMovies.Add(new UserMovie
        {
            UserId = userId,
            MovieId = movie.Id,
            IsWatchlist = true,
            AddedDate = DateTime.Now
        });

        await _context.SaveChangesAsync();
        _logger.LogInformation("Movie {TmdbId} added to watchlist for user {UserId}", tmdbId, userId);
        return true;
    }

    public async Task<bool> RemoveFromWatchlistAsync(string userId, int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null) return false;

        var userMovie = await _context.UserMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movie.Id);

        if (userMovie == null) return false;

        userMovie.IsWatchlist = false;

        if (!userMovie.IsFavorite)
            _context.UserMovies.Remove(userMovie);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Movie {TmdbId} removed from watchlist for user {UserId}", tmdbId, userId);
        return true;
    }

    public async Task<bool> IsInWatchlistAsync(string userId, int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null) return false;

        return await _context.UserMovies
            .AnyAsync(um => um.UserId == userId && um.MovieId == movie.Id && um.IsWatchlist);
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