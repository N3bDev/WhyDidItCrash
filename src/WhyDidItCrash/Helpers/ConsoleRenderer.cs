using WhyDidItCrash.Models;

namespace WhyDidItCrash.Helpers;

public static class ConsoleRenderer
{
    private static bool _noColor;

    public static void Render(DiagnosticReport report, bool noColor)
    {
        _noColor = noColor || Console.IsOutputRedirected;

        RenderHeader(report);
        RenderWarnings(report.Warnings);
        RenderSummary(report);
        RenderTimeline(report.Events);
        RenderHardwareHealth(report.HardwareHealth);
        RenderFooter();
    }

    public static string RenderToString(DiagnosticReport report)
    {
        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);
        _noColor = true;

        RenderHeader(report);
        RenderWarnings(report.Warnings);
        RenderSummary(report);
        RenderTimeline(report.Events);
        RenderHardwareHealth(report.HardwareHealth);
        RenderFooter();

        Console.SetOut(originalOut);
        return sw.ToString();
    }

    private static void RenderHeader(DiagnosticReport report)
    {
        Console.WriteLine();
        WriteColored("  ╔══════════════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
        WriteColored("  ║            WhyDidItCrash  -  Crash Diagnostic Report         ║", ConsoleColor.Cyan);
        WriteColored($"  ║            Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}                    ║", ConsoleColor.Cyan);
        WriteColored($"  ║            Lookback:  {report.LookbackDays} days                                  ║", ConsoleColor.Cyan);
        WriteColored("  ╚══════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
        Console.WriteLine();
    }

    private static void RenderWarnings(List<string> warnings)
    {
        if (warnings.Count == 0) return;

        foreach (var warning in warnings)
        {
            WriteColored($"  ⚠  {warning}", ConsoleColor.Yellow);
        }
        Console.WriteLine();
    }

    private static void RenderSummary(DiagnosticReport report)
    {
        var events = report.Events;
        var bsodCount = events.Count(e => e.Category == EventCategory.BSOD);
        var shutdownCount = events.Count(e => e.Category == EventCategory.UnexpectedShutdown);
        var appCrashCount = events.Count(e => e.Category is EventCategory.AppCrash or EventCategory.AppHang);
        var diskErrorCount = events.Count(e => e.Category == EventCategory.DiskError);
        var updateCount = events.Count(e => e.Category == EventCategory.WindowsUpdate);
        var criticalCount = events.Count(e => e.Severity == EventSeverity.Critical);

        WriteColored("  ┌─ SUMMARY ──────────────────────────────────────────────────┐", ConsoleColor.White);

        var totalCrashEvents = events.Count(e =>
            e.Category is EventCategory.BSOD or EventCategory.UnexpectedShutdown
                or EventCategory.AppCrash or EventCategory.AppHang or EventCategory.DiskError);

        var summaryColor = criticalCount > 0 ? ConsoleColor.Red :
            totalCrashEvents > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;

        WriteColored($"  │  Found {totalCrashEvents} crash/reboot event{(totalCrashEvents == 1 ? "" : "s")} in the last {report.LookbackDays} days", summaryColor);

        if (bsodCount > 0) WriteColored($"  │  ● {bsodCount} Blue Screen{(bsodCount == 1 ? "" : "s")} (BSOD)", ConsoleColor.Red);
        if (shutdownCount > 0) WriteColored($"  │  ● {shutdownCount} Unexpected Shutdown{(shutdownCount == 1 ? "" : "s")}", ConsoleColor.Red);
        if (appCrashCount > 0) WriteColored($"  │  ● {appCrashCount} Application Crash{(appCrashCount == 1 ? "" : "es")}", ConsoleColor.Yellow);
        if (diskErrorCount > 0) WriteColored($"  │  ● {diskErrorCount} Disk Error{(diskErrorCount == 1 ? "" : "s")}", ConsoleColor.Yellow);
        if (updateCount > 0) WriteColored($"  │  ● {updateCount} Windows Update event{(updateCount == 1 ? "" : "s")}", ConsoleColor.Gray);

        if (totalCrashEvents == 0)
            WriteColored("  │  No crash or reboot events found. System looks healthy!", ConsoleColor.Green);

        WriteColored("  └────────────────────────────────────────────────────────────┘", ConsoleColor.White);
        Console.WriteLine();
    }

    private static void RenderTimeline(List<CrashEvent> events)
    {
        if (events.Count == 0) return;

        WriteColored("  ═══ EVENT TIMELINE ══════════════════════════════════════════", ConsoleColor.Cyan);
        Console.WriteLine();

        foreach (var evt in events.OrderByDescending(e => e.Timestamp))
        {
            var severityColor = evt.Severity switch
            {
                EventSeverity.Critical => ConsoleColor.Red,
                EventSeverity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.Gray,
            };

            var severityLabel = evt.Severity switch
            {
                EventSeverity.Critical => "CRITICAL",
                EventSeverity.Warning => "WARNING",
                _ => "INFO",
            };

            var categoryLabel = evt.Category switch
            {
                EventCategory.BSOD => "BSOD",
                EventCategory.UnexpectedShutdown => "Unexpected Shutdown",
                EventCategory.PlannedShutdown => "Planned Shutdown",
                EventCategory.AppCrash => "App Crash",
                EventCategory.AppHang => "App Hang",
                EventCategory.DriverFailure => "Driver Failure",
                EventCategory.DiskError => "Disk Error",
                EventCategory.MemoryWarning => "Memory Warning",
                EventCategory.WindowsUpdate => "Windows Update",
                EventCategory.HardwareWarning => "Hardware Warning",
                _ => "Other",
            };

            var timestamp = evt.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            var relative = TimeHelpers.FormatRelativeTime(evt.Timestamp);

            WriteColored($"  [{timestamp}]  ●  {categoryLabel,-24} ({severityLabel})", severityColor);
            Console.WriteLine($"  {evt.ShortDescription}");

            if (!string.IsNullOrEmpty(evt.Explanation))
                Console.WriteLine($"  {evt.Explanation}");

            foreach (var detail in evt.Details)
                Console.WriteLine($"  {detail.Key}: {detail.Value}");

            if (!string.IsNullOrEmpty(evt.SuggestedAction))
                WriteColored($"  → {evt.SuggestedAction}", ConsoleColor.White);

            Console.WriteLine();
        }
    }

    private static void RenderHardwareHealth(HardwareHealthReport hw)
    {
        WriteColored("  ═══ HARDWARE HEALTH ═════════════════════════════════════════", ConsoleColor.Cyan);
        Console.WriteLine();

        // CPU
        Console.Write($"  CPU:          {hw.CpuName}");
        Console.WriteLine();
        if (hw.CpuTempCelsius.HasValue)
        {
            var temp = hw.CpuTempCelsius.Value;
            var (badge, color) = temp switch
            {
                >= 95 => ("[  HOT ]", ConsoleColor.Red),
                >= 80 => ("[ WARM ]", ConsoleColor.Yellow),
                _ => ("[  OK  ]", ConsoleColor.Green),
            };
            Console.Write($"  Temperature:  {temp:F0}°C  ");
            WriteColored(badge, color);
        }
        else
        {
            Console.Write("  Temperature:  ");
            WriteColored("N/A (sensor not available)", ConsoleColor.Gray);
        }

        Console.WriteLine();

        // Disks
        foreach (var disk in hw.Disks)
        {
            Console.WriteLine();
            Console.Write($"  Disk:         {disk.Model}");
            Console.WriteLine();

            if (disk.SmartPredictingFailure)
            {
                Console.Write("  SMART Status: FAILURE PREDICTED  ");
                WriteColored("[ FAIL ]", ConsoleColor.Red);
                WriteColored("  → Back up data immediately and replace this drive!", ConsoleColor.Red);
            }
            else
            {
                Console.Write($"  SMART Status: {disk.Status}  ");
                WriteColored("[  OK  ]", ConsoleColor.Green);
            }
        }

        Console.WriteLine();

        // Memory
        Console.WriteLine();
        Console.Write($"  Memory:       {hw.TotalMemoryMB / 1024.0:F0} GB ({hw.DimmCount} DIMM{(hw.DimmCount == 1 ? "" : "s")})");
        Console.WriteLine();
        if (hw.MemoryErrorsDetected)
        {
            Console.Write("  ECC Errors:   Errors detected!  ");
            WriteColored("[ FAIL ]", ConsoleColor.Red);
            WriteColored("  → Run Windows Memory Diagnostic or MemTest86.", ConsoleColor.Red);
        }
        else
        {
            Console.Write("  ECC Errors:   None detected  ");
            WriteColored("[  OK  ]", ConsoleColor.Green);
        }
        Console.WriteLine();

        // Battery
        Console.WriteLine();
        if (hw.BatteryHealthPercent.HasValue)
        {
            var health = hw.BatteryHealthPercent.Value;
            var (badge, color) = health switch
            {
                <= 25 => ("[ FAIL ]", ConsoleColor.Red),
                <= 50 => ("[ WARN ]", ConsoleColor.Yellow),
                _ => ("[  OK  ]", ConsoleColor.Green),
            };
            Console.Write($"  Battery:      {health:F0}% health  ");
            WriteColored(badge, color);

            if (health <= 25)
                WriteColored("  → Battery is severely degraded. Consider replacing it.", ConsoleColor.Red);
            else if (health <= 50)
                WriteColored("  → Battery is degraded. Plan for replacement.", ConsoleColor.Yellow);
        }
        else
        {
            Console.Write("  Battery:      ");
            WriteColored("Not present (desktop)", ConsoleColor.Gray);
        }
        Console.WriteLine();

        // Uptime
        Console.WriteLine();
        if (hw.Uptime.HasValue)
            Console.WriteLine($"  System Uptime: {TimeHelpers.FormatTimeSpan(hw.Uptime.Value)}");

        Console.WriteLine();
    }

    private static void RenderFooter()
    {
        WriteColored("  ═════════════════════════════════════════════════════════════", ConsoleColor.Cyan);
        Console.WriteLine("  Tip: Run as Administrator for the most complete results.");
        Console.WriteLine("  Tip: Use --export <file.txt> to save this report to a file.");
        Console.WriteLine();
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        if (_noColor)
        {
            Console.WriteLine(text);
            return;
        }

        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = prev;
    }
}
