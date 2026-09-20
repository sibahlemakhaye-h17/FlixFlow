using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FlixFlow.Interfaces;
using FlixFlow.Models;

namespace FlixFlow.Controllers;

[Authorize]
public class RatingsController : Controller
{
    private readonly IRatingService _ratingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RatingsController(
        IRatingService ratingService,
        UserManager<ApplicationUser> userManager)
    {
        _ratingService = ratingService;
        _userManager = userManager;
    }

    // POST: /Ratings/Rate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int tmdbId, int score)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { success = false, message = "Not authenticated" });

        var result = await _ratingService.RateMovieAsync(userId, tmdbId, score);

        if (!result)
            return Json(new { success = false, message = "Could not save rating" });

        var avg = await _ratingService.GetAverageRatingAsync(tmdbId);
        var count = await _ratingService.GetRatingCountAsync(tmdbId);

        return Json(new
        {
            success = true,
            score,
            average = avg?.ToString("0.0"),
            count,
            message = "Rating saved!"
        });
    }

    // POST: /Ratings/Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { success = false, message = "Not authenticated" });

        var result = await _ratingService.RemoveRatingAsync(userId, tmdbId);
        return Json(new { success = result });
    }

    // GET: /Ratings/GetUserRating?tmdbId=123
    [HttpGet]
    public async Task<IActionResult> GetUserRating(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { score = (int?)null });

        var score = await _ratingService.GetUserRatingAsync(userId, tmdbId);
        return Json(new { score });
    }
}
