namespace WhyDidItCrash.Models;

public sealed class TrendInsight
{
    public EventSeverity Severity { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string SuggestedAction { get; init; } = "";
}
