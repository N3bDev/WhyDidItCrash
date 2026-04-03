using System.Diagnostics.Eventing.Reader;
using WhyDidItCrash.Lookups;
using WhyDidItCrash.Models;

namespace WhyDidItCrash.Analyzers;

public sealed class EventLogAnalyzer : IAnalyzer
{
    public string Name => "Windows Event Log";

    public Task<IReadOnlyList<CrashEvent>> AnalyzeAsync(AppConfig config)
    {
        var events = new List<CrashEvent>();
        var cutoff = DateTime.Now.AddDays(-config.LookbackDays);

        AnalyzeSystemLog(events, cutoff);
        AnalyzeApplicationLog(events, cutoff);

        return Task.FromResult<IReadOnlyList<CrashEvent>>(events);
    }

    private static void AnalyzeSystemLog(List<CrashEvent> events, DateTime cutoff)
    {
        var millisecondsBack = (long)(DateTime.Now - cutoff).TotalMilliseconds;

        var query = $@"*[System[
            (
                (EventID=41 and Provider[@Name='Microsoft-Windows-Kernel-Power']) or
                (EventID=1001 and (Provider[@Name='Microsoft-Windows-WER-SystemErrorReporting'] or Provider[@Name='BugCheck'])) or
                (EventID=6008 and Provider[@Name='EventLog']) or
                (EventID=6006 and Provider[@Name='EventLog']) or
                (EventID=6005 and Provider[@Name='EventLog']) or
                (EventID=1074) or
                (EventID=7 and Provider[@Name='Disk']) or
                (EventID=11 and Provider[@Name='Disk']) or
                (EventID=15 and Provider[@Name='Disk']) or
                (EventID=51 and Provider[@Name='Disk']) or
                (EventID=153 and Provider[@Name='Disk']) or
                (EventID=19 and Provider[@Name='Microsoft-Windows-WindowsUpdateClient']) or
                (EventID=20 and Provider[@Name='Microsoft-Windows-WindowsUpdateClient']) or
                (EventID=43 and Provider[@Name='Microsoft-Windows-WindowsUpdateClient']) or
                (EventID=46 and Provider[@Name='volmgr'])
            )
            and TimeCreated[timediff(@SystemTime) <= {millisecondsBack}]
        ]]";

        ReadEventsFromLog("System", query, events);
    }

    private static void AnalyzeApplicationLog(List<CrashEvent> events, DateTime cutoff)
    {
        var millisecondsBack = (long)(DateTime.Now - cutoff).TotalMilliseconds;

        var query = $@"*[System[
            (
                (EventID=1000 and Provider[@Name='Application Error']) or
                (EventID=1002 and Provider[@Name='Application Hang']) or
                (EventID=1001 and Provider[@Name='Windows Error Reporting'])
            )
            and TimeCreated[timediff(@SystemTime) <= {millisecondsBack}]
        ]]";

        ReadEventsFromLog("Application", query, events);
    }

    private static void ReadEventsFromLog(string logName, string xpathQuery, List<CrashEvent> events)
    {
        try
        {
            var eventLogQuery = new EventLogQuery(logName, PathType.LogName, xpathQuery);
            using var reader = new EventLogReader(eventLogQuery);

            EventRecord? record;
            while ((record = reader.ReadEvent()) != null)
            {
                using (record)
                {
                    try
                    {
                        var evt = ConvertEventRecord(record);
                        if (evt != null)
                            events.Add(evt);
                    }
                    catch
                    {
                        // Skip corrupt or unreadable records
                    }
                }
            }
        }
        catch (EventLogNotFoundException)
        {
            // Log doesn't exist on this system
        }
        catch (UnauthorizedAccessException)
        {
            // Not running as admin
        }
        catch
        {
            // WMI/EventLog service issue
        }
    }

    private static CrashEvent? ConvertEventRecord(EventRecord record)
    {
        var providerName = record.ProviderName ?? "";
        var eventId = record.Id;
        var timestamp = record.TimeCreated ?? DateTime.MinValue;

        var info = EventIdLookup.GetInfo(providerName, eventId);
        if (info == null)
            return null;

        var details = new Dictionary<string, string>
        {
            ["Source"] = $"{providerName} (Event ID {eventId})",
            ["Log"] = record.LogName ?? "Unknown",
        };

        var shortDesc = $"Event ID {eventId}: {info.Description}";

        // Extract extra details for specific event types
        if (providerName == "Microsoft-Windows-WER-SystemErrorReporting" || providerName == "BugCheck")
        {
            shortDesc = ExtractBsodDetails(record, details);
        }
        else if (providerName == "Application Error" && eventId == 1000)
        {
            ExtractAppCrashDetails(record, details);
        }
        else if (eventId == 1074)
        {
            ExtractShutdownInitiator(record, details);
        }

        return new CrashEvent
        {
            Timestamp = timestamp,
            Category = info.Category,
            Severity = info.Severity,
            Source = "Event Log",
            ShortDescription = shortDesc,
            Explanation = info.Description,
            SuggestedAction = info.SuggestedAction,
            Details = details,
        };
    }

    private static string ExtractBsodDetails(EventRecord record, Dictionary<string, string> details)
    {
        try
        {
            var properties = record.Properties;
            if (properties.Count > 0)
            {
                // BugCheck event: Properties[0] = BugCheckCode
                var codeObj = properties[0].Value;
                if (codeObj is uint code || (codeObj is int intCode && (code = (uint)intCode) == (uint)intCode)
                    || (codeObj is long longCode && (code = (uint)longCode) == (uint)longCode))
                {
                    var bsodInfo = BsodCodeLookup.GetInfo(code);
                    details["Stop Code"] = $"0x{code:X8} ({bsodInfo.Name})";
                    return $"BSOD: 0x{code:X8} {bsodInfo.Name} -- {bsodInfo.Explanation}";
                }
            }
        }
        catch
        {
            // Properties not available
        }

        return "BSOD event recorded (stop code could not be extracted)";
    }

    private static void ExtractAppCrashDetails(EventRecord record, Dictionary<string, string> details)
    {
        try
        {
            var properties = record.Properties;
            if (properties.Count > 0)
                details["Faulting Application"] = properties[0].Value?.ToString() ?? "Unknown";
            if (properties.Count > 5)
                details["Faulting Module"] = properties[5].Value?.ToString() ?? "Unknown";
        }
        catch
        {
            // Properties not available
        }
    }

    private static void ExtractShutdownInitiator(EventRecord record, Dictionary<string, string> details)
    {
        try
        {
            var properties = record.Properties;
            if (properties.Count > 0)
                details["Initiated By"] = properties[0].Value?.ToString() ?? "Unknown";
            if (properties.Count > 4)
                details["Reason"] = properties[4].Value?.ToString() ?? "Unknown";
        }
        catch
        {
            // Properties not available
        }
    }
}
