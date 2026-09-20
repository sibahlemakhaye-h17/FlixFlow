using FlixFlow.Models;

namespace FlixFlow.Interfaces;

public interface IWatchlistService
{
    Task<List<Movie>> GetWatchlistAsync(string userId);
    Task<bool> AddToWatchlistAsync(string userId, int tmdbId);
    Task<bool> RemoveFromWatchlistAsync(string userId, int tmdbId);
    Task<bool> IsInWatchlistAsync(string userId, int tmdbId);
}