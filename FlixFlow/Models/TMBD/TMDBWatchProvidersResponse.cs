using System.Text.Json.Serialization;

namespace FlixFlow.Models.TMDB;

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
    public List<TMDBWatchProvider>? Flatrate { get; set; }

    [JsonPropertyName("rent")]
    public List<TMDBWatchProvider>? Rent { get; set; }

    [JsonPropertyName("buy")]
    public List<TMDBWatchProvider>? Buy { get; set; }
}

public class TMDBWatchProvider
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
