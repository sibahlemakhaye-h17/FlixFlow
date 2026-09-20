using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FlixFlow.Interfaces;
using FlixFlow.Models;

namespace FlixFlow.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesController(
        IFavoriteService favoriteService,
        UserManager<ApplicationUser> userManager)
    {
        _favoriteService = favoriteService;
        _userManager = userManager;
    }

    // GET: /Favorites
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var movies = await _favoriteService.GetFavoritesAsync(userId);
        return View(movies);
    }

    // POST: /Favorites/Add/123
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { success = false, message = "Not authenticated" });

        var result = await _favoriteService.AddToFavoritesAsync(userId, tmdbId);

        return Json(new
        {
            success = result,
            added = result,
            message = result ? "Added to favorites" : "Movie not found"
        });
    }

    // POST: /Favorites/Remove/123
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { success = false, message = "Not authenticated" });

        var result = await _favoriteService.RemoveFromFavoritesAsync(userId, tmdbId);

        return Json(new
        {
            success = result,
            added = false,
            message = result ? "Removed from favorites" : "Not in favorites"
        });
    }

    // GET: /Favorites/Check/123
    [HttpGet]
    public async Task<IActionResult> Check(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { isFavorite = false, isAuthenticated = false });

        var isFavorite = await _favoriteService.IsFavoriteAsync(userId, tmdbId);
        return Json(new { isFavorite, isAuthenticated = true });
    }
}
