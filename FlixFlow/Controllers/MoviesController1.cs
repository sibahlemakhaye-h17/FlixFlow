using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlixFlow.Data;
using FlixFlow.Interfaces;
using FlixFlow.Models;

namespace FlixFlow.Controllers;

public class MoviesController : Controller
{
    private readonly ITMDBService _tmdbService;
    private readonly ApplicationDbContext _context;
    private readonly IRatingService _ratingService;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(
        ITMDBService tmdbService,
        ApplicationDbContext context,
        IRatingService ratingService,
        ILogger<MoviesController> logger)
    {
        _tmdbService = tmdbService;
        _context = context;
        _ratingService = ratingService;
        _logger = logger;
    }

    // GET: /Movies
    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movies
            .OrderByDescending(m => m.VoteAverage)
            .Take(20)
            .ToListAsync();

        return View(movies);
    }

    // GET: /Movies/Popular
    public async Task<IActionResult> Popular(int page = 1)
    {
        var movies = await _tmdbService.GetPopularMoviesAsync(page);
        return View(movies);
    }

    // GET: /Movies/Search?query=xxx
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return RedirectToAction(nameof(Index));
        }

        var movies = await _tmdbService.SearchMoviesAsync(query);
        ViewBag.SearchQuery = query;
        return View(movies);
    }

    // GET: /Movies/Details/5
    public async Task<IActionResult> Details(int id)
    {
        // Try to get from database first
        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.TmdbId == id);

        if (movie == null)
        {
            // Fetch from TMDB
            movie = await _tmdbService.GetMovieDetailsAsync(id);

            if (movie != null)
            {
                // Save to database
                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();
            }
        }

        if (movie == null)
        {
            return NotFound();
        }

        // ============================================
        // LOAD TRAILER, PROVIDERS & RATINGS
        // ============================================
        ViewBag.TrailerKey = await _tmdbService.GetMovieTrailerKeyAsync(id);

        var providers = await _tmdbService.GetWatchProvidersAsync(id, "ZA");
        if (!providers.Any())
        {
            // Fallback to US if not available in ZA
            providers = await _tmdbService.GetWatchProvidersAsync(id, "US");
        }
        ViewBag.WatchProviders = providers;

        // Ratings
        ViewBag.AverageRating = await _ratingService.GetAverageRatingAsync(id);
        ViewBag.RatingCount = await _ratingService.GetRatingCountAsync(id);

        _logger.LogInformation($"Movie {id} - TrailerKey: {ViewBag.TrailerKey}, Providers: {providers.Count}");

        return View(movie);
    }
}