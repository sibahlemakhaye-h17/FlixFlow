namespace FlixFlow.ViewModels;

public class WatchProviderInfo
{
    public string ProviderName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;  // "Stream", "Rent", "Buy"
    public int DisplayPriority { get; set; }
}
