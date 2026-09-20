using System.Text.Json;
using System.Text.Json.Serialization;
using FlixFlow.Interfaces;
using FlixFlow.Models;
using FlixFlow.ViewModels;
using Microsoft.Extensions.Options;

namespace FlixFlow.Services;

public class TMDBService : ITMDBService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _imageBaseUrl;
    private readonly ILogger<TMDBService> _logger;

    public TMDBService(HttpClient httpClient, IOptions<TMDBOptions> options, ILogger<TMDBService> logger)
    {
        _httpClient = httpClient;
        _apiKey = options.Value.ApiKey;
        _imageBaseUrl = options.Value.ImageBaseUrl;
        _logger = logger;
        _logger.LogInformation($"TMDB Base URL: {_httpClient.BaseAddress}");
    }

    // ===============================================
    // POPULAR MOVIES
    // ===============================================
    public async Task<List<Movie>> GetPopularMoviesAsync(int page = 1)
    {
        var url = $"movie/popular?api_key={_apiKey}&page={page}";
        _logger.LogInformation($"Calling TMDB: {url}");

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"TMDB API Error: {response.StatusCode} - {errorContent}");
            response.EnsureSuccessStatusCode();
        }

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<TMDBResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return MapToMovies(result?.Results ?? new List<TMDBMovie>());
    }

    // ===============================================
    // SEARCH MOVIES
    // ===============================================
    public async Task<List<Movie>> SearchMoviesAsync(string query)
    {
        var url = $"search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}";
        _logger.LogInformation($"Calling TMDB: {url}");

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<TMDBResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return MapToMovies(result?.Results ?? new List<TMDBMovie>());
    }

    // ===============================================
    // MOVIE DETAILS
    // ===============================================
    public async Task<Movie?> GetMovieDetailsAsync(int tmdbId)
    {
        var url = $"movie/{tmdbId}?api_key={_apiKey}&append_to_response=videos";
        _logger.LogInformation($"Calling TMDB: {url}");

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        var movie = JsonSerializer.Deserialize<TMDBMovie>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (movie == null) return null;

        var videos = JsonSerializer.Deserialize<TMDBVideoResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var trailer = videos?.Results?.FirstOrDefault(v => v.Type == "Trailer" && v.Site == "YouTube");

        return new Movie
        {
            TmdbId = movie.Id,
            Title = movie.Title,
            Description = movie.Overview,
            ReleaseDate = movie.ReleaseDate,
            PosterPath = string.IsNullOrEmpty(movie.PosterPath) ? null : $"{_imageBaseUrl}/w500{movie.PosterPath}",
            BackdropPath = string.IsNullOrEmpty(movie.BackdropPath) ? null : $"{_imageBaseUrl}/w780{movie.BackdropPath}",
            VoteAverage = movie.VoteAverage,
            VoteCount = movie.VoteCount,
            Runtime = movie.Runtime,
            OriginalLanguage = movie.OriginalLanguage,
            TrailerUrl = trailer != null ? $"https://www.youtube.com/watch?v={trailer.Key}" : null
        };
    }

    // ===============================================
    // TRENDING MOVIES
    // ===============================================
    public async Task<List<Movie>> GetTrendingMoviesAsync()
    {
        var url = $"trending/movie/week?api_key={_apiKey}";
        _logger.LogInformation($"Calling TMDB: {url}");

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"TMDB Trending Error: {response.StatusCode} - {errorContent}");
            return await GetPopularMoviesAsync();
        }

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<TMDBResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return MapToMovies(result?.Results ?? new List<TMDBMovie>());
    }

    // ===============================================
    // UPCOMING MOVIES
    // ===============================================
    public async Task<List<Movie>> GetUpcomingMoviesAsync()
    {
        var url = $"movie/upcoming?api_key={_apiKey}";
        _logger.LogInformation($"Calling TMDB: {url}");

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"TMDB Upcoming Error: {response.StatusCode} - {errorContent}");
            return await GetPopularMoviesAsync();
        }

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<TMDBResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return MapToMovies(result?.Results ?? new List<TMDBMovie>());
    }

    // ===============================================
    // MOVIE TRAILER KEY (for embedding)
    // ===============================================
    public async Task<string?> GetMovieTrailerKeyAsync(int tmdbId)
    {
        var url = $"movie/{tmdbId}/videos?api_key={_apiKey}";
        _logger.LogInformation($"Calling TMDB: {url}");

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        var videos = JsonSerializer.Deserialize<TMDBVideoResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Prefer official YouTube trailers
        var trailer = videos?.Results?
            .FirstOrDefault(v => v.Type == "Trailer" && v.Site == "YouTube")
            ?? videos?.Results?.FirstOrDefault(v => v.Site == "YouTube");

        return trailer?.Key;
    }

    // ===============================================
    // WATCH PROVIDERS (Stream/Rent/Buy)
    // ===============================================
    public async Task<List<WatchProviderInfo>> GetWatchProvidersAsync(int tmdbId, string region = "ZA")
    {
        var url = $"movie/{tmdbId}/watch/providers?api_key={_apiKey}";
        _logger.LogInformation($"Calling TMDB: {url}");

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return new List<WatchProviderInfo>();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TMDBWatchProvidersResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result?.Results == null || !result.Results.ContainsKey(region))
            return new List<WatchProviderInfo>();

        var countryData = result.Results[region];
        var providers = new List<WatchProviderInfo>();

        // Streaming
        if (countryData.Flatrate != null)
        {
            foreach (var p in countryData.Flatrate)
            {
                providers.Add(new WatchProviderInfo
                {
                    ProviderName = p.ProviderName,
                    LogoUrl = $"https://image.tmdb.org/t/p/w92{p.LogoPath}",
                    Type = "Stream",
                    DisplayPriority = p.DisplayPriority
                });
            }
        }

        // Rent
        if (countryData.Rent != null)
        {
            foreach (var p in countryData.Rent)
            {
                providers.Add(new WatchProviderInfo
                {
                    ProviderName = p.ProviderName,
                    LogoUrl = $"https://image.tmdb.org/t/p/w92{p.LogoPath}",
                    Type = "Rent",
                    DisplayPriority = p.DisplayPriority
                });
            }
        }

        // Buy
        if (countryData.Buy != null)
        {
            foreach (var p in countryData.Buy)
            {
                providers.Add(new WatchProviderInfo
                {
                    ProviderName = p.ProviderName,
                    LogoUrl = $"https://image.tmdb.org/t/p/w92{p.LogoPath}",
                    Type = "Buy",
                    DisplayPriority = p.DisplayPriority
                });
            }
        }

        return providers;
    }

    // ===============================================
    // PRIVATE MAPPER
    // ===============================================
    private List<Movie> MapToMovies(List<TMDBMovie> tmdbMovies)
    {
        return tmdbMovies.Select(m => new Movie
        {
            TmdbId = m.Id,
            Title = m.Title,
            Description = m.Overview,
            ReleaseDate = m.ReleaseDate,
            PosterPath = string.IsNullOrEmpty(m.PosterPath) ? null : $"{_imageBaseUrl}/w500{m.PosterPath}",
            BackdropPath = string.IsNullOrEmpty(m.BackdropPath) ? null : $"{_imageBaseUrl}/w780{m.BackdropPath}",
            VoteAverage = m.VoteAverage,
            VoteCount = m.VoteCount,
            Runtime = m.Runtime,
            OriginalLanguage = m.OriginalLanguage
        }).ToList();
    }
}

// ===============================================
// TMDB API MODELS
// ===============================================

public class TMDBOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ImageBaseUrl { get; set; } = string.Empty;
}

public class TMDBResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("results")]
    public List<TMDBMovie> Results { get; set; } = new();

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }
}

public class TMDBMovie
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonPropertyName("release_date")]
    public DateTime? ReleaseDate { get; set; }

    [JsonPropertyName("poster_path")]
    public string PosterPath { get; set; } = string.Empty;

    [JsonPropertyName("backdrop_path")]
    public string BackdropPath { get; set; } = string.Empty;

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }

    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }

    [JsonPropertyName("runtime")]
    public int Runtime { get; set; }

    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; } = string.Empty;
}

public class TMDBVideoResponse
{
    [JsonPropertyName("results")]
    public List<TMDBVideo> Results { get; set; } = new();
}

public class TMDBVideo
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("site")]
    public string Site { get; set; } = string.Empty;
}

// ===============================================
// WATCH PROVIDERS MODELS
// ===============================================

public class TMDBWatchProvidersResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("results")]
    public Dictionary<string, TMDBWatchProviderCountry> Results { get; set; } = new();
}

public class TMDBWatchProviderCountry
{
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    [JsonPropertyName("flatrate")]
    public List<TMDBWatchProviderItem>? Flatrate { get; set; }

    [JsonPropertyName("rent")]
    public List<TMDBWatchProviderItem>? Rent { get; set; }

    [JsonPropertyName("buy")]
    public List<TMDBWatchProviderItem>? Buy { get; set; }
}

public class TMDBWatchProviderItem
{
    [JsonPropertyName("provider_id")]
    public int ProviderId { get; set; }

    [JsonPropertyName("provider_name")]
    public string ProviderName { get; set; } = string.Empty;

    [JsonPropertyName("logo_path")]
    public string LogoPath { get; set; } = string.Empty;

    [JsonPropertyName("display_priority")]
    public int DisplayPriority { get; set; }
}