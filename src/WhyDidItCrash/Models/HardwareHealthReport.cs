namespace WhyDidItCrash.Models;

public sealed class HardwareHealthReport
{
    public string CpuName { get; set; } = "Unknown";
    public double? CpuTempCelsius { get; set; }
    public List<DiskHealth> Disks { get; set; } = new();
    public long TotalMemoryMB { get; set; }
    public int DimmCount { get; set; }
    public bool MemoryErrorsDetected { get; set; }
    public double? BatteryHealthPercent { get; set; }
    public TimeSpan? Uptime { get; set; }
}

public sealed class DiskHealth
{
    public string Model { get; set; } = "";
    public bool SmartPredictingFailure { get; set; }
    public string Status { get; set; } = "";
}
