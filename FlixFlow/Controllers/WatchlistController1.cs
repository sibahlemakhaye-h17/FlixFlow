using FlixFlow.Interfaces;
using FlixFlow.Models;
using FlixFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlixFlow.Controllers;

[Authorize]
public class WatchlistController : Controller
{
    private readonly IWatchlistService _watchlistService;
    private readonly UserManager<ApplicationUser> _userManager;

    public WatchlistController(
        IWatchlistService watchlistService,
        UserManager<ApplicationUser> userManager)
    {
        _watchlistService = watchlistService;
        _userManager = userManager;
    }

    // GET: /Watchlist
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var movies = await _watchlistService.GetWatchlistAsync(userId);
        return View(movies);
    }

    // POST: /Watchlist/Add/123
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { success = false, message = "Not authenticated" });

        var result = await _watchlistService.AddToWatchlistAsync(userId, tmdbId);

        return Json(new
        {
            success = result,
            added = result,
            message = result ? "Added to watchlist" : "Movie not found"
        });
    }

    // POST: /Watchlist/Remove/123
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { success = false, message = "Not authenticated" });

        var result = await _watchlistService.RemoveFromWatchlistAsync(userId, tmdbId);

        return Json(new
        {
            success = result,
            added = false,
            message = result ? "Removed from watchlist" : "Not in watchlist"
        });
    }

    // GET: /Watchlist/Check/123
    [HttpGet]
    public async Task<IActionResult> Check(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { inWatchlist = false, isAuthenticated = false });

        var inWatchlist = await _watchlistService.IsInWatchlistAsync(userId, tmdbId);
        return Json(new { inWatchlist, isAuthenticated = true });
    }
}
