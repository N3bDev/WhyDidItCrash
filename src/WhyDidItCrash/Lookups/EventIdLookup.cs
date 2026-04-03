using WhyDidItCrash.Models;

namespace WhyDidItCrash.Lookups;

public static class EventIdLookup
{
    public record EventInfo(
        string Description,
        EventCategory Category,
        EventSeverity Severity,
        string SuggestedAction);

    private static readonly Dictionary<(string Provider, int EventId), EventInfo> Events = new()
    {
        // System log - Crash/Reboot events
        [("Microsoft-Windows-Kernel-Power", 41)] = new(
            "The system rebooted without cleanly shutting down first (unexpected power loss or crash).",
            EventCategory.UnexpectedShutdown,
            EventSeverity.Critical,
            "Check for BSODs around the same time. Inspect power supply and cables. Check for overheating."),

        [("Microsoft-Windows-WER-SystemErrorReporting", 1001)] = new(
            "A blue screen (BSOD) was recorded by the system.",
            EventCategory.BSOD,
            EventSeverity.Critical,
            "See the associated stop code for specific guidance."),

        [("BugCheck", 1001)] = new(
            "A blue screen (BSOD) was recorded by the system.",
            EventCategory.BSOD,
            EventSeverity.Critical,
            "See the associated stop code for specific guidance."),

        // EventLog service events
        [("EventLog", 6008)] = new(
            "The previous system shutdown was unexpected (dirty shutdown detected on boot).",
            EventCategory.UnexpectedShutdown,
            EventSeverity.Critical,
            "Investigate the cause -- power loss, freeze, or crash. Check other events around the same time."),

        [("EventLog", 6006)] = new(
            "The Event Log service was stopped (clean shutdown).",
            EventCategory.PlannedShutdown,
            EventSeverity.Info,
            "Informational only -- this indicates a normal shutdown."),

        [("EventLog", 6005)] = new(
            "The Event Log service was started (system boot).",
            EventCategory.PlannedShutdown,
            EventSeverity.Info,
            "Informational only -- this indicates a normal startup."),

        [("USER32", 1074)] = new(
            "A process or user initiated a system shutdown or restart.",
            EventCategory.PlannedShutdown,
            EventSeverity.Info,
            "Check which process initiated the restart (often Windows Update or an installer)."),

        // Disk errors
        [("Disk", 7)] = new(
            "A bad block (sector) was encountered on the disk.",
            EventCategory.DiskError,
            EventSeverity.Warning,
            "Run 'chkdsk /f /r'. Check SMART status. Consider replacing the drive if errors persist."),

        [("Disk", 11)] = new(
            "The disk driver detected a controller error on the disk.",
            EventCategory.DiskError,
            EventSeverity.Warning,
            "Check SATA/USB cables. Run 'chkdsk /f /r'. Check SMART status."),

        [("Disk", 15)] = new(
            "The disk is not ready for access yet.",
            EventCategory.DiskError,
            EventSeverity.Warning,
            "Check disk connections. May indicate a failing drive or loose cable."),

        [("Disk", 51)] = new(
            "An error was detected on the disk during a paging operation.",
            EventCategory.DiskError,
            EventSeverity.Warning,
            "Run 'chkdsk /f /r'. Check SMART status. Consider replacing the drive."),

        [("Disk", 153)] = new(
            "The IO operation at a logical block address was retried.",
            EventCategory.DiskError,
            EventSeverity.Warning,
            "This can indicate a slow or failing disk. Check SMART status."),

        // Windows Update
        [("Microsoft-Windows-WindowsUpdateClient", 19)] = new(
            "A Windows Update was successfully installed (may have triggered a restart).",
            EventCategory.WindowsUpdate,
            EventSeverity.Info,
            "Informational -- this may explain a recent reboot."),

        [("Microsoft-Windows-WindowsUpdateClient", 20)] = new(
            "A Windows Update installation failed.",
            EventCategory.WindowsUpdate,
            EventSeverity.Warning,
            "Re-run Windows Update. Check C:\\Windows\\Logs\\CBS for details."),

        [("Microsoft-Windows-WindowsUpdateClient", 43)] = new(
            "Windows Update started downloading an update.",
            EventCategory.WindowsUpdate,
            EventSeverity.Info,
            "Informational only."),

        // Crash dump
        [("volmgr", 46)] = new(
            "Crash dump initialization failed -- crash dumps will not be created.",
            EventCategory.Other,
            EventSeverity.Warning,
            "Enable crash dumps in System Properties > Advanced > Startup and Recovery."),

        // Application log events
        [("Application Error", 1000)] = new(
            "An application crashed due to an unhandled exception.",
            EventCategory.AppCrash,
            EventSeverity.Warning,
            "Update or reinstall the faulting application. Check for compatibility issues."),

        [("Application Hang", 1002)] = new(
            "An application stopped responding and was closed.",
            EventCategory.AppHang,
            EventSeverity.Warning,
            "Update the application. Check for resource exhaustion (RAM, CPU)."),

        [("Windows Error Reporting", 1001)] = new(
            "Windows Error Reporting logged a fault bucket for an application crash.",
            EventCategory.AppCrash,
            EventSeverity.Info,
            "Check the associated Application Error event for more details."),
    };

    public static EventInfo? GetInfo(string providerName, int eventId)
    {
        if (Events.TryGetValue((providerName, eventId), out var info))
            return info;

        return null;
    }
}
