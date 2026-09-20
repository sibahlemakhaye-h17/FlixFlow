using Microsoft.EntityFrameworkCore;
using FlixFlow.Data;
using FlixFlow.Interfaces;
using FlixFlow.Models;

namespace FlixFlow.Services;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _context;
    private readonly ITMDBService _tmdbService;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(
        ApplicationDbContext context,
        ITMDBService tmdbService,
        ILogger<FavoriteService> logger)
    {
        _context = context;
        _tmdbService = tmdbService;
        _logger = logger;
    }

    public async Task<List<Movie>> GetFavoritesAsync(string userId)
    {
        return await _context.UserMovies
            .Include(um => um.Movie)
            .Where(um => um.UserId == userId && um.IsFavorite)
            .OrderByDescending(um => um.AddedDate)
            .Select(um => um.Movie)
            .ToListAsync();
    }

    public async Task<bool> AddToFavoritesAsync(string userId, int tmdbId)
    {
        var movie = await GetOrCreateMovieAsync(tmdbId);
        if (movie == null) return false;

        var existing = await _context.UserMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movie.Id);

        if (existing != null)
        {
            if (existing.IsFavorite) return true;

            existing.IsFavorite = true;
            await _context.SaveChangesAsync();
            return true;
        }

        _context.UserMovies.Add(new UserMovie
        {
            UserId = userId,
            MovieId = movie.Id,
            IsFavorite = true,
            AddedDate = DateTime.Now
        });

        await _context.SaveChangesAsync();
        _logger.LogInformation("Movie {TmdbId} added to favorites for user {UserId}", tmdbId, userId);
        return true;
    }

    public async Task<bool> RemoveFromFavoritesAsync(string userId, int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null) return false;

        var userMovie = await _context.UserMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movie.Id);

        if (userMovie == null) return false;

        userMovie.IsFavorite = false;

        if (!userMovie.IsWatchlist)
            _context.UserMovies.Remove(userMovie);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Movie {TmdbId} removed from favorites for user {UserId}", tmdbId, userId);
        return true;
    }

    public async Task<bool> IsFavoriteAsync(string userId, int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null) return false;

        return await _context.UserMovies
            .AnyAsync(um => um.UserId == userId && um.MovieId == movie.Id && um.IsFavorite);
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
