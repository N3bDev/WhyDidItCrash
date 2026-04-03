namespace WhyDidItCrash.Models;

public sealed class DiagnosticReport
{
    public DateTime GeneratedAt { get; init; }
    public int LookbackDays { get; init; }
    public List<CrashEvent> Events { get; init; } = new();
    public HardwareHealthReport HardwareHealth { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
}
