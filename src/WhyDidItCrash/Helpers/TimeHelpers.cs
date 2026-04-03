using System.Management;

namespace WhyDidItCrash.Helpers;

public static class TimeHelpers
{
    public static string FormatTimeSpan(TimeSpan ts)
    {
        var parts = new List<string>();
        if (ts.Days > 0)
            parts.Add($"{ts.Days} day{(ts.Days == 1 ? "" : "s")}");
        if (ts.Hours > 0)
            parts.Add($"{ts.Hours} hour{(ts.Hours == 1 ? "" : "s")}");
        if (ts.Minutes > 0)
            parts.Add($"{ts.Minutes} minute{(ts.Minutes == 1 ? "" : "s")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "less than a minute";
    }

    public static string FormatRelativeTime(DateTime timestamp)
    {
        var diff = DateTime.Now - timestamp;
        if (diff.TotalMinutes < 1)
            return "just now";
        if (diff.TotalHours < 1)
            return $"{(int)diff.TotalMinutes} minute{((int)diff.TotalMinutes == 1 ? "" : "s")} ago";
        if (diff.TotalDays < 1)
            return $"{(int)diff.TotalHours} hour{((int)diff.TotalHours == 1 ? "" : "s")} ago";
        if (diff.TotalDays < 30)
            return $"{(int)diff.TotalDays} day{((int)diff.TotalDays == 1 ? "" : "s")} ago";

        return timestamp.ToString("yyyy-MM-dd HH:mm");
    }

    public static DateTime? ParseWmiDateTime(string wmiDateTime)
    {
        try
        {
            return ManagementDateTimeConverter.ToDateTime(wmiDateTime);
        }
        catch
        {
            return null;
        }
    }
}
