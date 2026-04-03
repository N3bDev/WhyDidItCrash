using WhyDidItCrash;
using WhyDidItCrash.Analyzers;
using WhyDidItCrash.Helpers;
using WhyDidItCrash.Models;

var config = ParseArguments(args);

if (config == null)
{
    PrintUsage();
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

// Deduplicate events that are within 5 seconds of each other with the same category
allEvents = DeduplicateEvents(allEvents);

// Build report
var report = new DiagnosticReport
{
    GeneratedAt = DateTime.Now,
    LookbackDays = config.LookbackDays,
    Events = allEvents,
    HardwareHealth = hardwareHealth ?? new HardwareHealthReport(),
    Warnings = warnings,
};

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

return 0;

// --- Helper methods ---

static AppConfig? ParseArguments(string[] args)
{
    int lookbackDays = 30;
    bool noColor = false;
    string? exportPath = null;

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i].ToLowerInvariant())
        {
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
    };
}

static void PrintUsage()
{
    Console.WriteLine();
    Console.WriteLine("  WhyDidItCrash - Windows Crash Diagnostic Tool");
    Console.WriteLine();
    Console.WriteLine("  Usage: WhyDidItCrash.exe [options]");
    Console.WriteLine();
    Console.WriteLine("  Options:");
    Console.WriteLine("    --days, -d <N>       Look back N days (default: 30)");
    Console.WriteLine("    --no-color           Disable colored output");
    Console.WriteLine("    --export, -e <path>  Export report to a text file");
    Console.WriteLine("    --help, -h           Show this help message");
    Console.WriteLine();
    Console.WriteLine("  Examples:");
    Console.WriteLine("    WhyDidItCrash.exe                    Scan last 30 days");
    Console.WriteLine("    WhyDidItCrash.exe --days 7           Scan last 7 days");
    Console.WriteLine("    WhyDidItCrash.exe -e report.txt      Save report to file");
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
            Math.Abs((existing.Timestamp - evt.Timestamp).TotalSeconds) < 5 &&
            existing.ShortDescription == evt.ShortDescription);

        if (!isDuplicate)
            deduplicated.Add(evt);
    }

    return deduplicated;
}
