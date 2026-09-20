using FlixFlow.Models;

namespace FlixFlow.Interfaces;

public interface IFavoriteService
{
    Task<List<Movie>> GetFavoritesAsync(string userId);
    Task<bool> AddToFavoritesAsync(string userId, int tmdbId);
    Task<bool> RemoveFromFavoritesAsync(string userId, int tmdbId);
    Task<bool> IsFavoriteAsync(string userId, int tmdbId);
}
