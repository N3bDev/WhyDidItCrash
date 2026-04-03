using WhyDidItCrash.Models;

namespace WhyDidItCrash.Analyzers;

public static class TrendAnalyzer
{
    public static List<TrendInsight> Analyze(List<CrashEvent> events, int lookbackDays)
    {
        var insights = new List<TrendInsight>();

        DetectRecurringCrashes(events, insights);
        DetectFrequencyTrend(events, lookbackDays, insights);
        DetectCorrelations(events, insights);

        return insights;
    }

    private static void DetectRecurringCrashes(List<CrashEvent> events, List<TrendInsight> insights)
    {
        // Only look at system-level events for recurring pattern detection
        var systemEvents = events.Where(e =>
            e.Category is EventCategory.BSOD or EventCategory.UnexpectedShutdown
                or EventCategory.DiskError or EventCategory.MemoryWarning
                or EventCategory.HardwareWarning or EventCategory.DriverFailure);

        var groups = systemEvents
            .GroupBy(e => (e.Category, e.ShortDescription))
            .Where(g => g.Count() >= 3)
            .OrderByDescending(g => g.Count());

        foreach (var group in groups)
        {
            var count = group.Count();
            var category = group.Key.Category;
            var first = group.OrderBy(e => e.Timestamp).First();
            var last = group.OrderByDescending(e => e.Timestamp).First();
            var span = last.Timestamp - first.Timestamp;

            var categoryName = category switch
            {
                EventCategory.BSOD => "BSOD",
                EventCategory.UnexpectedShutdown => "unexpected shutdown",
                EventCategory.DiskError => "disk error",
                EventCategory.MemoryWarning => "memory warning",
                EventCategory.HardwareWarning => "hardware warning",
                EventCategory.DriverFailure => "driver failure",
                _ => "event",
            };

            insights.Add(new TrendInsight
            {
                Severity = count >= 5 ? EventSeverity.Critical : EventSeverity.Warning,
                Title = $"Recurring {categoryName} ({count}x)",
                Description = $"The same {categoryName} has occurred {count} times over {FormatSpan(span)}. Pattern: {Truncate(group.Key.ShortDescription, 100)}",
                SuggestedAction = first.SuggestedAction,
            });
        }
    }

    private static void DetectFrequencyTrend(List<CrashEvent> events, int lookbackDays, List<TrendInsight> insights)
    {
        // Compare crash rate in first half vs second half of the lookback period
        var systemEvents = events.Where(e =>
            e.Category is EventCategory.BSOD or EventCategory.UnexpectedShutdown
                or EventCategory.DiskError or EventCategory.MemoryWarning
                or EventCategory.HardwareWarning or EventCategory.DriverFailure)
            .ToList();

        if (systemEvents.Count < 3) return;

        var midpoint = DateTime.Now.AddDays(-lookbackDays / 2.0);
        var firstHalf = systemEvents.Count(e => e.Timestamp < midpoint);
        var secondHalf = systemEvents.Count(e => e.Timestamp >= midpoint);

        if (secondHalf > firstHalf && secondHalf >= 3)
        {
            insights.Add(new TrendInsight
            {
                Severity = secondHalf >= firstHalf * 2 ? EventSeverity.Critical : EventSeverity.Warning,
                Title = "Increasing crash frequency",
                Description = $"System issues are escalating: {firstHalf} event{(firstHalf == 1 ? "" : "s")} in the first half vs {secondHalf} in the recent half of the lookback period.",
                SuggestedAction = "Investigate recent changes — new drivers, Windows updates, or hardware changes may be the cause.",
            });
        }
    }

    private static void DetectCorrelations(List<CrashEvent> events, List<TrendInsight> insights)
    {
        // Look for disk errors preceding BSODs or unexpected shutdowns within 60 seconds
        var bsodsAndShutdowns = events
            .Where(e => e.Category is EventCategory.BSOD or EventCategory.UnexpectedShutdown)
            .ToList();

        var diskErrors = events
            .Where(e => e.Category == EventCategory.DiskError)
            .ToList();

        if (bsodsAndShutdowns.Count == 0 || diskErrors.Count == 0) return;

        var correlationCount = 0;
        foreach (var crash in bsodsAndShutdowns)
        {
            var hasPrecedingDiskError = diskErrors.Any(d =>
                d.Timestamp < crash.Timestamp &&
                (crash.Timestamp - d.Timestamp).TotalSeconds <= 60);

            if (hasPrecedingDiskError)
                correlationCount++;
        }

        if (correlationCount >= 2)
        {
            insights.Add(new TrendInsight
            {
                Severity = EventSeverity.Critical,
                Title = "Disk errors correlated with crashes",
                Description = $"Disk errors preceded {correlationCount} crash{(correlationCount == 1 ? "" : "es")}, suggesting a failing drive may be causing system instability.",
                SuggestedAction = "Check disk SMART status immediately. Back up critical data and consider replacing the drive.",
            });
        }

        // Look for memory warnings preceding BSODs
        var memEvents = events
            .Where(e => e.Category == EventCategory.MemoryWarning)
            .ToList();

        if (memEvents.Count > 0 && bsodsAndShutdowns.Count > 0)
        {
            var memCorrelation = 0;
            foreach (var crash in bsodsAndShutdowns)
            {
                var hasPrecedingMemError = memEvents.Any(m =>
                    m.Timestamp < crash.Timestamp &&
                    (crash.Timestamp - m.Timestamp).TotalSeconds <= 120);

                if (hasPrecedingMemError)
                    memCorrelation++;
            }

            if (memCorrelation >= 2)
            {
                insights.Add(new TrendInsight
                {
                    Severity = EventSeverity.Critical,
                    Title = "Memory errors correlated with crashes",
                    Description = $"Memory warnings preceded {memCorrelation} crash{(memCorrelation == 1 ? "" : "es")}. Faulty RAM is likely the root cause.",
                    SuggestedAction = "Run MemTest86 overnight. Reseat RAM sticks. Test each stick individually.",
                });
            }
        }
    }

    private static string FormatSpan(TimeSpan span)
    {
        if (span.TotalDays >= 1)
            return $"{(int)span.TotalDays} day{((int)span.TotalDays == 1 ? "" : "s")}";
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours} hour{((int)span.TotalHours == 1 ? "" : "s")}";
        return $"{(int)span.TotalMinutes} minute{((int)span.TotalMinutes == 1 ? "" : "s")}";
    }

    private static string Truncate(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        return text[..(maxLength - 3)] + "...";
    }
}
