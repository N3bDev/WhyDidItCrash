using WhyDidItCrash.Models;

namespace WhyDidItCrash.Helpers;

public static class HealthScorer
{
    public static (int Score, string Verdict) Evaluate(DiagnosticReport report)
    {
        var score = 100;
        var events = report.Events;

        // Penalize system-level events heavily
        foreach (var evt in events)
        {
            score -= evt.Category switch
            {
                EventCategory.BSOD => 15,
                EventCategory.UnexpectedShutdown => 10,
                EventCategory.DiskError => 10,
                EventCategory.MemoryWarning => 10,
                EventCategory.HardwareWarning => 10,
                EventCategory.DriverFailure => 8,
                EventCategory.AppCrash => 2,
                EventCategory.AppHang => 2,
                _ => 0,
            };
        }

        // Hardware health penalties
        var hw = report.HardwareHealth;
        if (hw.MemoryErrorsDetected)
            score -= 20;
        if (hw.Disks.Any(d => d.SmartPredictingFailure))
            score -= 25;
        if (hw.CpuTempCelsius is >= 95)
            score -= 15;
        else if (hw.CpuTempCelsius is >= 80)
            score -= 5;
        if (hw.BatteryHealthPercent is <= 25)
            score -= 10;

        score = Math.Max(0, score);

        var verdict = BuildVerdict(events, hw, score);
        return (score, verdict);
    }

    private static string BuildVerdict(List<CrashEvent> events, HardwareHealthReport hw, int score)
    {
        if (score >= 90)
            return "System looks healthy. No significant issues detected.";

        // Find the dominant issue
        var bsodCount = events.Count(e => e.Category == EventCategory.BSOD);
        var shutdownCount = events.Count(e => e.Category == EventCategory.UnexpectedShutdown);
        var diskCount = events.Count(e => e.Category == EventCategory.DiskError);
        var memCount = events.Count(e => e.Category == EventCategory.MemoryWarning);
        var hwCount = events.Count(e => e.Category == EventCategory.HardwareWarning);

        // Check hardware failures first — they're the root cause of many crashes
        if (hw.Disks.Any(d => d.SmartPredictingFailure))
            return "A disk is reporting imminent failure (SMART alert). Back up data immediately and replace the drive.";

        if (hw.MemoryErrorsDetected)
            return "Memory hardware errors detected. Faulty RAM can cause BSODs and data corruption. Test and replace RAM.";

        if (hw.CpuTempCelsius is >= 95)
            return "CPU temperature is critically high. This can cause crashes and hardware damage. Check cooling immediately.";

        // Check for recurring BSOD pattern
        if (bsodCount >= 3)
        {
            var topBsod = events
                .Where(e => e.Category == EventCategory.BSOD)
                .GroupBy(e => e.ShortDescription)
                .OrderByDescending(g => g.Count())
                .First();

            if (topBsod.Count() >= 2)
            {
                var driverDetail = topBsod.First().Details
                    .Where(d => d.Key.Contains("Driver", StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.Value)
                    .FirstOrDefault();

                if (driverDetail != null)
                    return $"Recurring BSOD crashes ({topBsod.Count()}x) likely caused by {driverDetail}. Priority: update or replace this driver.";

                return $"Recurring BSOD crashes ({topBsod.Count()}x): {Truncate(topBsod.Key, 80)}. Check the event details for the faulting component.";
            }

            return $"Multiple BSOD crashes detected ({bsodCount}x). Review the timeline for common stop codes or faulting drivers.";
        }

        if (bsodCount > 0)
            return "BSOD crash detected. Check the stop code and faulting driver in the timeline below.";

        if (diskCount >= 5)
            return $"High volume of disk errors ({diskCount}x). Drive may be failing. Check SMART status and run chkdsk /f /r.";

        if (diskCount > 0 && shutdownCount > 0)
            return "Disk errors and unexpected shutdowns detected — these may be related. Check drive health and power supply.";

        if (shutdownCount >= 3)
            return $"Multiple unexpected shutdowns ({shutdownCount}x). Check power supply, overheating, or driver issues.";

        if (shutdownCount > 0)
            return "Unexpected shutdown detected. Check for power issues, overheating, or driver problems.";

        if (memCount > 0)
            return "Memory warnings detected. Run Windows Memory Diagnostic or MemTest86 to test RAM.";

        if (hw.CpuTempCelsius is >= 80)
            return "CPU temperature is elevated. Ensure adequate cooling to prevent thermal throttling and crashes.";

        if (diskCount > 0)
            return $"Disk errors detected ({diskCount}x). Monitor drive health and consider running chkdsk.";

        if (hwCount > 0)
            return "Hardware warnings detected. Review the hardware health section below.";

        return "Some minor issues found. Review the timeline for details.";
    }

    private static string Truncate(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        return text[..(maxLength - 3)] + "...";
    }
}
