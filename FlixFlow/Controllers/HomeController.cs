using Microsoft.AspNetCore.Mvc;
using FlixFlow.Interfaces;

namespace FlixFlow.Controllers;

public class HomeController : Controller
{
    private readonly ITMDBService _tmdbService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ITMDBService tmdbService, ILogger<HomeController> logger)
    {
        _tmdbService = tmdbService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var trending = await _tmdbService.GetTrendingMoviesAsync();
        var upcoming = await _tmdbService.GetUpcomingMoviesAsync();

        ViewBag.Trending = trending.Take(10);
        ViewBag.Upcoming = upcoming.Take(8);

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
