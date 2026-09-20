using FlixFlow.Models;
using FlixFlow.ViewModels;

namespace FlixFlow.Interfaces;

public interface ITMDBService
{
    Task<List<Movie>> GetPopularMoviesAsync(int page = 1);
    Task<List<Movie>> SearchMoviesAsync(string query);
    Task<Movie?> GetMovieDetailsAsync(int tmdbId);
    Task<List<Movie>> GetTrendingMoviesAsync();
    Task<List<Movie>> GetUpcomingMoviesAsync();
    Task<string?> GetMovieTrailerKeyAsync(int tmdbId);
    Task<List<WatchProviderInfo>> GetWatchProvidersAsync(int tmdbId, string region = "ZA");
}