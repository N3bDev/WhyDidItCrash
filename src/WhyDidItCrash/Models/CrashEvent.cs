namespace WhyDidItCrash.Models;

public sealed class CrashEvent
{
    public DateTime Timestamp { get; init; }
    public EventCategory Category { get; init; }
    public EventSeverity Severity { get; init; }
    public string Source { get; init; } = "";
    public string ShortDescription { get; init; } = "";
    public string Explanation { get; init; } = "";
    public string SuggestedAction { get; init; } = "";
    public Dictionary<string, string> Details { get; init; } = new();
}
