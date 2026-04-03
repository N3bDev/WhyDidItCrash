using WhyDidItCrash;
using WhyDidItCrash.Analyzers;
using WhyDidItCrash.Helpers;
using WhyDidItCrash.Models;

var config = ParseArguments(args);

if (config == null)
{
    PrintUsage();
    WaitForKeypress();
    return 1;
}

// Check elevation
ElevationHelper.WarnIfNotElevated();

// Run analyzers
var warnings = new List<string>();
var allEvents = new List<CrashEvent>();

var analyzers = new IAnalyzer[]
{
    new EventLogAnalyzer(),
    new MinidumpAnalyzer(),
    new HardwareAnalyzer(),
};

HardwareHealthReport? hardwareHealth = null;

foreach (var analyzer in analyzers)
{
    try
    {
        Console.Write($"  Scanning {analyzer.Name}...");
        var events = await analyzer.AnalyzeAsync(config);
        allEvents.AddRange(events);
        Console.WriteLine(" done.");

        if (analyzer is HardwareAnalyzer hw)
            hardwareHealth = hw.HealthReport;
    }
    catch (UnauthorizedAccessException)
    {
        warnings.Add($"Could not access {analyzer.Name} (requires Administrator).");
        Console.WriteLine(" skipped (needs admin).");
    }
    catch (Exception ex)
    {
        warnings.Add($"Error scanning {analyzer.Name}: {ex.Message}");
        Console.WriteLine(" error.");
    }
}

if (!ElevationHelper.IsElevated())
    warnings.Add("Running without Administrator privileges. Some data sources may be incomplete.");

// Sort events chronologically (newest first)
allEvents.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

// Deduplicate events within the deduplication window
allEvents = DeduplicateEvents(allEvents);

// Apply filter if specified
if (!string.IsNullOrEmpty(config.Filter))
    allEvents = ApplyFilter(allEvents, config.Filter);

// Build report
var report = new DiagnosticReport
{
    GeneratedAt = DateTime.Now,
    LookbackDays = config.LookbackDays,
    Events = allEvents,
    HardwareHealth = hardwareHealth ?? new HardwareHealthReport(),
    Warnings = warnings,
};

// Score system health
var (healthScore, verdict) = HealthScorer.Evaluate(report);
report.HealthScore = healthScore;
report.Verdict = verdict;

// Analyze trends and patterns
report.Insights = TrendAnalyzer.Analyze(allEvents, config.LookbackDays);

// Render
ConsoleRenderer.Render(report, config.NoColor);

// Export if requested
if (!string.IsNullOrEmpty(config.ExportPath))
{
    try
    {
        var text = ConsoleRenderer.RenderToString(report);
        File.WriteAllText(config.ExportPath, text);
        Console.WriteLine($"  Report exported to: {config.ExportPath}");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Failed to export report: {ex.Message}");
        Console.ResetColor();
    }
}

WaitForKeypress();

return 0;

// --- Helper methods ---

static void WaitForKeypress()
{
    if (!Console.IsOutputRedirected)
    {
        Console.WriteLine();
        Console.WriteLine("  Press any key to exit...");
        Console.ReadKey(true);
    }
}

static AppConfig? ParseArguments(string[] args)
{
    int lookbackDays = 30;
    bool noColor = false;
    string? exportPath = null;
    string? filter = null;

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i].ToLowerInvariant())
        {
            case "--version" or "-v":
                Console.WriteLine($"  WhyDidItCrash v{Constants.Version}");
                Environment.Exit(0);
                break;

            case "--days" or "-d":
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out var days) && days > 0)
                {
                    lookbackDays = days;
                    i++;
                }
                else
                {
                    Console.WriteLine("Error: --days requires a positive integer value.");
                    return null;
                }
                break;

            case "--no-color":
                noColor = true;
                break;

            case "--export" or "-e":
                if (i + 1 < args.Length)
                {
                    exportPath = args[i + 1];
                    i++;
                }
                else
                {
                    Console.WriteLine("Error: --export requires a file path.");
                    return null;
                }
                break;

            case "--filter" or "-f":
                if (i + 1 < args.Length)
                {
                    filter = args[i + 1];
                    i++;
                }
                else
                {
                    Console.WriteLine("Error: --filter requires a value (e.g., critical, system, bsod,disk).");
                    return null;
                }
                break;

            case "--help" or "-h" or "-?" or "/?":
                return null;

            default:
                Console.WriteLine($"Unknown option: {args[i]}");
                return null;
        }
    }

    return new AppConfig
    {
        LookbackDays = lookbackDays,
        NoColor = noColor,
        ExportPath = exportPath,
        Filter = filter,
    };
}

static void PrintUsage()
{
    Console.WriteLine();
    Console.WriteLine($"  WhyDidItCrash v{Constants.Version} - Windows Crash Diagnostic Tool");
    Console.WriteLine();
    Console.WriteLine("  Usage: WhyDidItCrash.exe [options]");
    Console.WriteLine();
    Console.WriteLine("  Options:");
    Console.WriteLine("    --days, -d <N>       Look back N days (default: 30)");
    Console.WriteLine("    --filter, -f <type>  Filter events: critical, system, bsod, disk, memory,");
    Console.WriteLine("                         hardware, shutdown, app (comma-separated for multiple)");
    Console.WriteLine("    --no-color           Disable colored output");
    Console.WriteLine("    --export, -e <path>  Export report to a text file");
    Console.WriteLine("    --version, -v        Show version");
    Console.WriteLine("    --help, -h           Show this help message");
    Console.WriteLine();
    Console.WriteLine("  Examples:");
    Console.WriteLine("    WhyDidItCrash.exe                          Scan last 30 days");
    Console.WriteLine("    WhyDidItCrash.exe --days 7                 Scan last 7 days");
    Console.WriteLine("    WhyDidItCrash.exe --filter system          System events only (no app crashes)");
    Console.WriteLine("    WhyDidItCrash.exe --filter critical        Critical severity only");
    Console.WriteLine("    WhyDidItCrash.exe --filter bsod,disk       BSODs and disk errors only");
    Console.WriteLine("    WhyDidItCrash.exe -e report.txt            Save report to file");
    Console.WriteLine();
    Console.WriteLine("  For best results, run as Administrator (right-click > Run as administrator).");
    Console.WriteLine();
}

static List<CrashEvent> DeduplicateEvents(List<CrashEvent> events)
{
    var deduplicated = new List<CrashEvent>();

    foreach (var evt in events)
    {
        var isDuplicate = deduplicated.Any(existing =>
            existing.Category == evt.Category &&
            Math.Abs((existing.Timestamp - evt.Timestamp).TotalSeconds) < Constants.DeduplicationWindowSeconds &&
            existing.ShortDescription == evt.ShortDescription);

        if (!isDuplicate)
            deduplicated.Add(evt);
    }

    return deduplicated;
}

static List<CrashEvent> ApplyFilter(List<CrashEvent> events, string filter)
{
    var filters = filter.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    return events.Where(e =>
    {
        foreach (var f in filters)
        {
            var match = f switch
            {
                "critical" => e.Severity == EventSeverity.Critical,
                "system" => e.Category is EventCategory.BSOD or EventCategory.UnexpectedShutdown
                    or EventCategory.DiskError or EventCategory.MemoryWarning
                    or EventCategory.HardwareWarning or EventCategory.DriverFailure,
                "bsod" => e.Category == EventCategory.BSOD,
                "disk" => e.Category == EventCategory.DiskError,
                "memory" => e.Category == EventCategory.MemoryWarning,
                "hardware" => e.Category == EventCategory.HardwareWarning,
                "shutdown" => e.Category is EventCategory.UnexpectedShutdown or EventCategory.PlannedShutdown,
                "app" => e.Category is EventCategory.AppCrash or EventCategory.AppHang,
                _ => true,
            };
            if (match) return true;
        }
        return false;
    }).ToList();
}
