namespace WhyDidItCrash;

public static class Constants
{
    // Temperature thresholds (Celsius)
    public const double CpuTempCritical = 95;
    public const double CpuTempWarning = 80;

    // Battery health thresholds (percent)
    public const double BatteryHealthCritical = 25;
    public const double BatteryHealthWarning = 50;

    // Event deduplication window (seconds)
    public const double DeduplicationWindowSeconds = 5;

    // Minidump analysis buffer size (bytes)
    public const int MinidumpScanBufferSize = 65536;

    // Version
    public const string Version = "2.0.0";
}
