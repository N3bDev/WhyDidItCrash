using WhyDidItCrash;
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
        RenderInsights(report.Insights);
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
        RenderInsights(report.Insights);
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
        var memoryCount = events.Count(e => e.Category == EventCategory.MemoryWarning);
        var hwCount = events.Count(e => e.Category == EventCategory.HardwareWarning);
        var updateCount = events.Count(e => e.Category == EventCategory.WindowsUpdate);

        WriteColored("  ┌─ SYSTEM HEALTH ─────────────────────────────────────────────┐", ConsoleColor.White);

        // Health score badge
        var score = report.HealthScore;
        var (scoreLabel, scoreColor) = score switch
        {
            >= 90 => ("HEALTHY", ConsoleColor.Green),
            >= 60 => ("WARNING", ConsoleColor.Yellow),
            >= 30 => ("DEGRADED", ConsoleColor.Red),
            _ => ("CRITICAL", ConsoleColor.DarkRed),
        };
        WriteColored($"  │  Health Score: {score}/100  [{scoreLabel}]", scoreColor);
        Console.WriteLine();

        // Verdict
        if (!string.IsNullOrEmpty(report.Verdict))
            WriteColored($"  │  {report.Verdict}", scoreColor);

        Console.WriteLine();

        // System event counts (prioritized)
        var systemEvents = events.Count(e =>
            e.Category is EventCategory.BSOD or EventCategory.UnexpectedShutdown
                or EventCategory.DiskError or EventCategory.MemoryWarning
                or EventCategory.HardwareWarning or EventCategory.DriverFailure);

        if (bsodCount > 0) WriteColored($"  │  ● {bsodCount} Blue Screen{(bsodCount == 1 ? "" : "s")} (BSOD)", ConsoleColor.Red);
        if (shutdownCount > 0) WriteColored($"  │  ● {shutdownCount} Unexpected Shutdown{(shutdownCount == 1 ? "" : "s")}", ConsoleColor.Red);
        if (diskErrorCount > 0) WriteColored($"  │  ● {diskErrorCount} Disk Error{(diskErrorCount == 1 ? "" : "s")}", ConsoleColor.Yellow);
        if (memoryCount > 0) WriteColored($"  │  ● {memoryCount} Memory Warning{(memoryCount == 1 ? "" : "s")}", ConsoleColor.Yellow);
        if (hwCount > 0) WriteColored($"  │  ● {hwCount} Hardware Warning{(hwCount == 1 ? "" : "s")}", ConsoleColor.Yellow);
        if (appCrashCount > 0) WriteColored($"  │  ● {appCrashCount} Application Crash{(appCrashCount == 1 ? "" : "es")}", ConsoleColor.Gray);
        if (updateCount > 0) WriteColored($"  │  ● {updateCount} Windows Update event{(updateCount == 1 ? "" : "s")}", ConsoleColor.DarkGray);

        if (systemEvents == 0 && appCrashCount == 0)
            WriteColored("  │  No crash or error events found.", ConsoleColor.Green);

        WriteColored("  └────────────────────────────────────────────────────────────┘", ConsoleColor.White);
        Console.WriteLine();
    }

    private static void RenderInsights(List<TrendInsight> insights)
    {
        if (insights.Count == 0) return;

        WriteColored("  ═══ INSIGHTS ════════════════════════════════════════════════", ConsoleColor.Magenta);
        Console.WriteLine();

        foreach (var insight in insights)
        {
            var color = insight.Severity switch
            {
                EventSeverity.Critical => ConsoleColor.Red,
                EventSeverity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.Gray,
            };

            WriteColored($"  ▸ {insight.Title}", color);
            Console.WriteLine($"    {insight.Description}");
            if (!string.IsNullOrEmpty(insight.SuggestedAction))
                WriteColored($"    → {insight.SuggestedAction}", ConsoleColor.White);
            Console.WriteLine();
        }
    }

    private static void RenderTimeline(List<CrashEvent> events)
    {
        if (events.Count == 0) return;

        // Tier 1: System-level events (always shown in full)
        var systemEvents = events
            .Where(e => e.Category is EventCategory.BSOD or EventCategory.UnexpectedShutdown
                or EventCategory.DiskError or EventCategory.MemoryWarning
                or EventCategory.HardwareWarning or EventCategory.DriverFailure)
            .OrderByDescending(e => e.Timestamp)
            .ToList();

        // Tier 2: Application events (condensed)
        var appEvents = events
            .Where(e => e.Category is EventCategory.AppCrash or EventCategory.AppHang)
            .ToList();

        // Tier 3: Informational events (compact)
        var infoEvents = events
            .Where(e => e.Category is EventCategory.PlannedShutdown or EventCategory.WindowsUpdate or EventCategory.Other)
            .OrderByDescending(e => e.Timestamp)
            .ToList();

        // Render system events
        if (systemEvents.Count > 0)
        {
            WriteColored("  ═══ SYSTEM EVENTS ═══════════════════════════════════════════", ConsoleColor.Cyan);
            Console.WriteLine();

            foreach (var evt in systemEvents)
                RenderEventFull(evt);
        }

        // Render app events (condensed — grouped by app name)
        if (appEvents.Count > 0)
        {
            WriteColored("  ═══ APPLICATION EVENTS ══════════════════════════════════════", ConsoleColor.Gray);
            Console.WriteLine();

            var appGroups = appEvents
                .GroupBy(e => e.Details.GetValueOrDefault("Faulting Application", e.ShortDescription))
                .OrderByDescending(g => g.Count());

            foreach (var group in appGroups)
            {
                var count = group.Count();
                var latest = group.OrderByDescending(e => e.Timestamp).First();
                var categoryLabel = latest.Category == EventCategory.AppHang ? "Hang" : "Crash";

                if (count >= 5)
                {
                    // Frequent crasher — show detail
                    WriteColored($"  ● {group.Key} — {count} {categoryLabel}{(count == 1 ? "" : "es")}", ConsoleColor.Yellow);
                    Console.WriteLine($"    Last: {latest.Timestamp:yyyy-MM-dd HH:mm:ss} ({TimeHelpers.FormatRelativeTime(latest.Timestamp)})");
                    if (latest.Details.TryGetValue("Faulting Module", out var module))
                        Console.WriteLine($"    Faulting Module: {module}");
                    WriteColored($"    → {latest.SuggestedAction}", ConsoleColor.White);
                    Console.WriteLine();
                }
                else
                {
                    // Infrequent — single line
                    Console.WriteLine($"  ● {group.Key} — {count} {categoryLabel}{(count == 1 ? "" : "es")} (last: {TimeHelpers.FormatRelativeTime(latest.Timestamp)})");
                }
            }
            Console.WriteLine();
        }

        // Render info events (compact — single line each)
        if (infoEvents.Count > 0)
        {
            WriteColored("  ─── Informational ──────────────────────────────────────────", ConsoleColor.DarkGray);
            foreach (var evt in infoEvents)
            {
                Console.WriteLine($"  {evt.Timestamp:yyyy-MM-dd HH:mm}  {FormatCategoryLabel(evt.Category)}: {evt.ShortDescription}");
            }
            Console.WriteLine();
        }
    }

    private static void RenderEventFull(CrashEvent evt)
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

        var timestamp = evt.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

        WriteColored($"  [{timestamp}]  ●  {FormatCategoryLabel(evt.Category),-24} ({severityLabel})", severityColor);
        Console.WriteLine($"  {evt.ShortDescription}");

        if (!string.IsNullOrEmpty(evt.Explanation))
            Console.WriteLine($"  {evt.Explanation}");

        foreach (var detail in evt.Details)
            Console.WriteLine($"  {detail.Key}: {detail.Value}");

        if (!string.IsNullOrEmpty(evt.SuggestedAction))
            WriteColored($"  → {evt.SuggestedAction}", ConsoleColor.White);

        Console.WriteLine();
    }

    private static string FormatCategoryLabel(EventCategory category)
    {
        return category switch
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
                >= Constants.CpuTempCritical => ("[  HOT ]", ConsoleColor.Red),
                >= Constants.CpuTempWarning => ("[ WARM ]", ConsoleColor.Yellow),
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
                <= Constants.BatteryHealthCritical => ("[ FAIL ]", ConsoleColor.Red),
                <= Constants.BatteryHealthWarning => ("[ WARN ]", ConsoleColor.Yellow),
                _ => ("[  OK  ]", ConsoleColor.Green),
            };
            Console.Write($"  Battery:      {health:F0}% health  ");
            WriteColored(badge, color);

            if (health <= Constants.BatteryHealthCritical)
                WriteColored("  → Battery is severely degraded. Consider replacing it.", ConsoleColor.Red);
            else if (health <= Constants.BatteryHealthWarning)
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
