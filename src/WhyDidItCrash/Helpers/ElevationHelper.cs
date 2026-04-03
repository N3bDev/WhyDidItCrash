using System.Security.Principal;

namespace WhyDidItCrash.Helpers;

public static class ElevationHelper
{
    public static bool IsElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void WarnIfNotElevated()
    {
        if (IsElevated())
            return;

        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  WARNING: Not running as Administrator.");
        Console.WriteLine("  Some data sources (minidumps, WMI SMART data) may be inaccessible.");
        Console.WriteLine("  For best results, right-click the .exe and select 'Run as administrator'.");
        Console.ForegroundColor = prevColor;
        Console.WriteLine();
    }
}
