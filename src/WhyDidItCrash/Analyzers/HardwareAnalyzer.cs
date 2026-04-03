using System.Management;
using WhyDidItCrash.Helpers;
using WhyDidItCrash.Models;

namespace WhyDidItCrash.Analyzers;

public sealed class HardwareAnalyzer : IAnalyzer
{
    public string Name => "Hardware Health";

    public HardwareHealthReport HealthReport { get; private set; } = new();

    public Task<IReadOnlyList<CrashEvent>> AnalyzeAsync(AppConfig config)
    {
        var events = new List<CrashEvent>();
        var report = new HardwareHealthReport();

        CheckCpu(report);
        CheckTemperature(report, events);
        CheckDisks(report, events);
        CheckMemory(report, events);
        CheckBattery(report, events);
        CheckUptime(report);

        HealthReport = report;
        return Task.FromResult<IReadOnlyList<CrashEvent>>(events);
    }

    private static void CheckCpu(HardwareHealthReport report)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\cimv2",
                "SELECT Name FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                report.CpuName = obj["Name"]?.ToString()?.Trim() ?? "Unknown";
                break;
            }
        }
        catch
        {
            // WMI not available
        }
    }

    private static void CheckTemperature(HardwareHealthReport report, List<CrashEvent> events)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\WMI",
                "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");
            foreach (var obj in searcher.Get())
            {
                var tempKelvinTenths = Convert.ToDouble(obj["CurrentTemperature"]);
                var celsius = (tempKelvinTenths / 10.0) - 273.15;
                report.CpuTempCelsius = celsius;

                if (celsius >= 95)
                {
                    events.Add(new CrashEvent
                    {
                        Timestamp = DateTime.Now,
                        Category = EventCategory.HardwareWarning,
                        Severity = EventSeverity.Critical,
                        Source = "WMI",
                        ShortDescription = $"CPU temperature is critically high: {celsius:F0}°C",
                        Explanation = "The CPU is running dangerously hot. This can cause thermal throttling, instability, and sudden shutdowns.",
                        SuggestedAction = "Check CPU cooler mounting and thermal paste. Clean dust from fans and heatsinks. Ensure adequate case airflow.",
                    });
                }
                else if (celsius >= 80)
                {
                    events.Add(new CrashEvent
                    {
                        Timestamp = DateTime.Now,
                        Category = EventCategory.HardwareWarning,
                        Severity = EventSeverity.Warning,
                        Source = "WMI",
                        ShortDescription = $"CPU temperature is elevated: {celsius:F0}°C",
                        Explanation = "The CPU is running warmer than ideal. This may contribute to instability under heavy load.",
                        SuggestedAction = "Clean dust from fans and heatsinks. Ensure adequate case airflow. Consider reapplying thermal paste.",
                    });
                }

                break; // Use first thermal zone
            }
        }
        catch
        {
            // Thermal zone not available on many systems
        }
    }

    private static void CheckDisks(HardwareHealthReport report, List<CrashEvent> events)
    {
        // Get disk models
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\cimv2",
                "SELECT Model, Status FROM Win32_DiskDrive");
            foreach (var obj in searcher.Get())
            {
                var disk = new DiskHealth
                {
                    Model = obj["Model"]?.ToString() ?? "Unknown",
                    Status = obj["Status"]?.ToString() ?? "Unknown",
                };
                report.Disks.Add(disk);
            }
        }
        catch
        {
            // WMI not available
        }

        // Check SMART failure prediction
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\WMI",
                "SELECT PredictFailure, InstanceName FROM MSStorageDriver_FailurePredictStatus");
            var diskIndex = 0;
            foreach (var obj in searcher.Get())
            {
                var predictFailure = (bool)obj["PredictFailure"];
                if (predictFailure)
                {
                    if (diskIndex < report.Disks.Count)
                        report.Disks[diskIndex].SmartPredictingFailure = true;

                    var diskName = diskIndex < report.Disks.Count
                        ? report.Disks[diskIndex].Model
                        : "Unknown disk";

                    events.Add(new CrashEvent
                    {
                        Timestamp = DateTime.Now,
                        Category = EventCategory.HardwareWarning,
                        Severity = EventSeverity.Critical,
                        Source = "WMI (SMART)",
                        ShortDescription = $"SMART predicts failure for: {diskName}",
                        Explanation = "The drive's own firmware is predicting imminent hardware failure. Data loss is likely.",
                        SuggestedAction = "BACK UP ALL DATA IMMEDIATELY. Replace this drive as soon as possible.",
                    });
                }
                diskIndex++;
            }
        }
        catch
        {
            // SMART data not accessible (may need admin)
        }
    }

    private static void CheckMemory(HardwareHealthReport report, List<CrashEvent> events)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\cimv2",
                "SELECT Capacity FROM Win32_PhysicalMemory");
            long totalBytes = 0;
            int dimmCount = 0;
            foreach (var obj in searcher.Get())
            {
                var capacity = Convert.ToInt64(obj["Capacity"]);
                totalBytes += capacity;
                dimmCount++;
            }
            report.TotalMemoryMB = totalBytes / (1024 * 1024);
            report.DimmCount = dimmCount;
        }
        catch
        {
            // WMI not available
        }

        // Check for memory errors
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\cimv2",
                "SELECT ErrorInfo FROM Win32_MemoryDevice");
            foreach (var obj in searcher.Get())
            {
                var errorInfo = obj["ErrorInfo"];
                if (errorInfo != null)
                {
                    var errorCode = Convert.ToInt32(errorInfo);
                    if (errorCode > 0 && errorCode != 2) // 2 = unknown, 0 = none
                    {
                        report.MemoryErrorsDetected = true;
                        events.Add(new CrashEvent
                        {
                            Timestamp = DateTime.Now,
                            Category = EventCategory.MemoryWarning,
                            Severity = EventSeverity.Critical,
                            Source = "WMI",
                            ShortDescription = "Memory errors detected in hardware",
                            Explanation = "The system has detected errors in physical memory. This can cause BSODs and data corruption.",
                            SuggestedAction = "Run Windows Memory Diagnostic (mdsched.exe) or MemTest86. Reseat or replace faulty RAM sticks.",
                        });
                        break;
                    }
                }
            }
        }
        catch
        {
            // Win32_MemoryDevice not available on all systems
        }
    }

    private static void CheckBattery(HardwareHealthReport report, List<CrashEvent> events)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\cimv2",
                "SELECT DesignCapacity, FullChargeCapacity FROM Win32_Battery");
            foreach (var obj in searcher.Get())
            {
                var designCapacity = Convert.ToDouble(obj["DesignCapacity"] ?? 0);
                var fullChargeCapacity = Convert.ToDouble(obj["FullChargeCapacity"] ?? 0);

                if (designCapacity > 0 && fullChargeCapacity > 0)
                {
                    var healthPercent = (fullChargeCapacity / designCapacity) * 100.0;
                    report.BatteryHealthPercent = healthPercent;

                    if (healthPercent <= 25)
                    {
                        events.Add(new CrashEvent
                        {
                            Timestamp = DateTime.Now,
                            Category = EventCategory.HardwareWarning,
                            Severity = EventSeverity.Critical,
                            Source = "WMI",
                            ShortDescription = $"Battery health is critically low: {healthPercent:F0}%",
                            Explanation = "The battery has degraded severely and may cause unexpected shutdowns when on battery power.",
                            SuggestedAction = "Replace the battery. Avoid running on battery power until replaced.",
                        });
                    }
                }

                break; // Only check first battery
            }
        }
        catch
        {
            // No battery or WMI not available
        }
    }

    private static void CheckUptime(HardwareHealthReport report)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\cimv2",
                "SELECT LastBootUpTime FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                var lastBootStr = obj["LastBootUpTime"]?.ToString();
                if (lastBootStr != null)
                {
                    var lastBoot = TimeHelpers.ParseWmiDateTime(lastBootStr);
                    if (lastBoot.HasValue)
                        report.Uptime = DateTime.Now - lastBoot.Value;
                }
                break;
            }
        }
        catch
        {
            // WMI not available
        }
    }
}
