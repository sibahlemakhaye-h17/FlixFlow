namespace FlixFlow.Interfaces;

public interface IRatingService
{
    Task<int?> GetUserRatingAsync(string userId, int tmdbId);
    Task<double?> GetAverageRatingAsync(int tmdbId);
    Task<int> GetRatingCountAsync(int tmdbId);
    Task<bool> RateMovieAsync(string userId, int tmdbId, int score);
    Task<bool> RemoveRatingAsync(string userId, int tmdbId);
    Task<int> GetUserRatingsCountAsync(string userId);
}
