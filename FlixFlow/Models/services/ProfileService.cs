using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlixFlow.Data;
using FlixFlow.Interfaces;
using FlixFlow.Models;
using FlixFlow.ViewModels;

namespace FlixFlow.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ProfileService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ProfileViewModel?> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var stats = await GetUserStatsAsync(userId);

        return new ProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            CreatedAt = user.CreatedAt,
            IsPremium = user.IsPremium,
            ProfilePictureUrl = user.ProfilePictureUrl,
            WatchlistCount = stats.WatchlistCount,
            FavoritesCount = stats.FavoritesCount,
            RatingsCount = stats.RatingsCount
        };
    }

    public async Task<bool> UpdateProfileAsync(string userId, ProfileViewModel model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            _logger.LogInformation("Profile updated for user {UserId}", userId);

        return result.Succeeded;
    }

    // Private helper
    private async Task<(int WatchlistCount, int FavoritesCount, int RatingsCount)> GetUserStatsAsync(string userId)
    {
        var userMovies = await _context.UserMovies
            .Where(um => um.UserId == userId)
            .ToListAsync();

        var ratingsCount = await _context.Ratings
            .CountAsync(r => r.UserId == userId);

        return (
            userMovies.Count(um => um.IsWatchlist),
            userMovies.Count(um => um.IsFavorite),
            ratingsCount
        );
    }
}

