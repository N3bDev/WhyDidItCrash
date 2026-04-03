namespace WhyDidItCrash;

public sealed class AppConfig
{
    public int LookbackDays { get; init; } = 30;
    public bool NoColor { get; init; }
    public string? ExportPath { get; init; }
}
