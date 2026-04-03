using System.Text.RegularExpressions;
using WhyDidItCrash.Lookups;
using WhyDidItCrash.Models;

namespace WhyDidItCrash.Analyzers;

public sealed partial class MinidumpAnalyzer : IAnalyzer
{
    public string Name => "Minidump Files";

    private static readonly string MinidumpPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Minidump");

    public Task<IReadOnlyList<CrashEvent>> AnalyzeAsync(AppConfig config)
    {
        var events = new List<CrashEvent>();
        var cutoff = DateTime.Now.AddDays(-config.LookbackDays);

        try
        {
            if (!Directory.Exists(MinidumpPath))
                return Task.FromResult<IReadOnlyList<CrashEvent>>(events);

            var dmpFiles = Directory.GetFiles(MinidumpPath, "*.dmp")
                .Where(f => File.GetLastWriteTime(f) >= cutoff)
                .OrderByDescending(f => File.GetLastWriteTime(f));

            foreach (var file in dmpFiles)
            {
                try
                {
                    var evt = ParseDumpFile(file);
                    if (evt != null)
                        events.Add(evt);
                }
                catch
                {
                    // Skip unreadable dump files
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Not running as admin -- can't access Minidump folder
        }
        catch (DirectoryNotFoundException)
        {
            // Minidump folder doesn't exist
        }

        return Task.FromResult<IReadOnlyList<CrashEvent>>(events);
    }

    private static CrashEvent? ParseDumpFile(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new BinaryReader(stream);

        if (stream.Length < 8)
            return null;

        var signature = reader.ReadBytes(4);
        var sigString = System.Text.Encoding.ASCII.GetString(signature);

        uint bugCheckCode;
        ulong param1 = 0, param2 = 0, param3 = 0, param4 = 0;

        if (sigString == "PAGE")
        {
            // Full or kernel memory dump - DUMP_HEADER format
            bugCheckCode = ParseKernelDump(reader, out param1, out param2, out param3, out param4);
        }
        else if (sigString == "MDMP")
        {
            // Minidump format
            bugCheckCode = ParseMinidump(reader);
        }
        else
        {
            return null;
        }

        if (bugCheckCode == 0)
            return null;

        var bsodInfo = BsodCodeLookup.GetInfo(bugCheckCode);
        var faultingDriver = FindFaultingDriver(filePath);
        var fileTime = File.GetLastWriteTime(filePath);

        var details = new Dictionary<string, string>
        {
            ["Source"] = $"Minidump: {Path.GetFileName(filePath)}",
            ["Stop Code"] = $"0x{bugCheckCode:X8} ({bsodInfo.Name})",
        };

        if (param1 != 0)
            details["Parameter 1"] = $"0x{param1:X16}";

        if (!string.IsNullOrEmpty(faultingDriver))
            details["Probable Faulting Driver"] = faultingDriver;

        var shortDesc = $"BSOD: 0x{bugCheckCode:X8} {bsodInfo.Name}";
        if (!string.IsNullOrEmpty(faultingDriver))
            shortDesc += $" (driver: {faultingDriver})";

        return new CrashEvent
        {
            Timestamp = fileTime,
            Category = EventCategory.BSOD,
            Severity = EventSeverity.Critical,
            Source = "Minidump",
            ShortDescription = shortDesc,
            Explanation = bsodInfo.Explanation,
            SuggestedAction = bsodInfo.SuggestedAction,
            Details = details,
        };
    }

    private static uint ParseKernelDump(BinaryReader reader, out ulong param1, out ulong param2, out ulong param3, out ulong param4)
    {
        param1 = param2 = param3 = param4 = 0;

        try
        {
            // DUMP_HEADER64: BugCheckCode at offset 0x060
            reader.BaseStream.Seek(0x060, SeekOrigin.Begin);
            var bugCheckCode = reader.ReadUInt32();

            // Skip 4 bytes padding
            reader.ReadUInt32();

            // Bug check parameters at 0x068, 0x070, 0x078, 0x080
            param1 = reader.ReadUInt64();
            param2 = reader.ReadUInt64();
            param3 = reader.ReadUInt64();
            param4 = reader.ReadUInt64();

            return bugCheckCode;
        }
        catch
        {
            return 0;
        }
    }

    private static uint ParseMinidump(BinaryReader reader)
    {
        try
        {
            // MDMP header:
            // Offset 4-5: Version (uint16)
            // Offset 8-11: NumberOfStreams (uint32)
            // Offset 12-15: StreamDirectoryRva (uint32)
            reader.BaseStream.Seek(4, SeekOrigin.Begin);
            reader.ReadUInt16(); // version
            reader.ReadUInt16(); // implementation specific
            var numberOfStreams = reader.ReadUInt32();
            var streamDirectoryRva = reader.ReadUInt32();

            // Walk the stream directory to find ExceptionStream (type 6)
            reader.BaseStream.Seek(streamDirectoryRva, SeekOrigin.Begin);

            for (uint i = 0; i < numberOfStreams; i++)
            {
                var streamType = reader.ReadUInt32();
                var dataSize = reader.ReadUInt32();
                var rva = reader.ReadUInt32();

                if (streamType == 6) // ExceptionStream
                {
                    // MINIDUMP_EXCEPTION_STREAM:
                    // Offset 0: ThreadId (uint32)
                    // Offset 4: __alignment (uint32)
                    // Offset 8: MINIDUMP_EXCEPTION record
                    //   Offset 8+0: ExceptionCode (uint32)
                    reader.BaseStream.Seek(rva + 8, SeekOrigin.Begin);
                    return reader.ReadUInt32();
                }
            }
        }
        catch
        {
            // Malformed dump
        }

        return 0;
    }

    private static string? FindFaultingDriver(string filePath)
    {
        try
        {
            // Read first 64KB and scan for driver names (.sys files)
            var bytesToRead = Math.Min(65536, new FileInfo(filePath).Length);
            var buffer = new byte[bytesToRead];

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            var text = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);

            // Find .sys driver references
            var matches = DriverPattern().Matches(text);
            if (matches.Count == 0)
                return null;

            // Count occurrences, exclude common system drivers
            var systemDrivers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ntoskrnl.sys", "ntkrnlmp.sys", "ntkrnlpa.sys", "ntkrpamp.sys",
                "hal.sys", "ntfs.sys", "fltmgr.sys", "ci.sys", "ksecdd.sys",
                "clfs.sys", "tm.sys", "wdf01000.sys", "wdfldr.sys",
            };

            var driverCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in matches)
            {
                var driver = match.Value.ToLowerInvariant();
                if (!systemDrivers.Contains(driver))
                    driverCounts[driver] = driverCounts.GetValueOrDefault(driver) + 1;
            }

            // Return the most frequently referenced non-system driver
            return driverCounts
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    [GeneratedRegex(@"\b\w+\.sys\b", RegexOptions.IgnoreCase)]
    private static partial Regex DriverPattern();
}
