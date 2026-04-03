namespace WhyDidItCrash.Lookups;

public static class BsodCodeLookup
{
    public record BsodInfo(string Name, string Explanation, string SuggestedAction);

    private static readonly Dictionary<uint, BsodInfo> Codes = new()
    {
        [0x0000000A] = new(
            "IRQL_NOT_LESS_OR_EQUAL",
            "A driver tried to access invalid memory at too high a priority level. Usually caused by a faulty or incompatible driver.",
            "Update or roll back recently installed drivers. Test RAM with Windows Memory Diagnostic."),

        [0x0000001E] = new(
            "KMODE_EXCEPTION_NOT_HANDLED",
            "A kernel-mode program generated an exception that the error handler didn't catch.",
            "Check for driver updates. Run 'sfc /scannow' to repair system files."),

        [0x00000024] = new(
            "NTFS_FILE_SYSTEM",
            "A problem occurred within the NTFS file system driver, often due to disk corruption or a failing drive.",
            "Run 'chkdsk /f /r' on the system drive. Check disk SMART health. Replace SATA cables if applicable."),

        [0x0000002E] = new(
            "DATA_BUS_ERROR",
            "A parity error in system memory was detected, indicating a RAM hardware fault.",
            "Run Windows Memory Diagnostic or MemTest86. Reseat RAM sticks. Test individual sticks."),

        [0x0000003B] = new(
            "SYSTEM_SERVICE_EXCEPTION",
            "An exception happened while executing a system service routine. Often caused by GPU drivers or antivirus software.",
            "Update GPU drivers. Disable/uninstall third-party antivirus temporarily. Run 'sfc /scannow'."),

        [0x0000003F] = new(
            "NO_MORE_SYSTEM_PTES",
            "The system ran out of page table entries. A driver is leaking kernel resources.",
            "Identify the leaking driver. Update or remove the offending driver."),

        [0x00000050] = new(
            "PAGE_FAULT_IN_NONPAGED_AREA",
            "The system tried to access memory that should always be available but wasn't. Common causes: bad RAM, corrupt drivers, or faulty disk.",
            "Test RAM with MemTest86. Run 'chkdsk /f /r'. Update or roll back recently changed drivers."),

        [0x00000051] = new(
            "REGISTRY_ERROR",
            "A severe registry error occurred -- the hive file could not be read or is corrupted. Often caused by disk corruption.",
            "Run 'sfc /scannow' and 'chkdsk /f'. Check disk SMART health. Consider System Restore."),

        [0x0000007A] = new(
            "KERNEL_DATA_INPAGE_ERROR",
            "The system could not read required data from disk into memory. Usually a failing drive, bad sector, or loose cable.",
            "Run 'chkdsk /f /r'. Test RAM. Check/replace SATA cables. Scan for malware."),

        [0x0000007E] = new(
            "SYSTEM_THREAD_EXCEPTION_NOT_HANDLED",
            "A system thread generated an exception that wasn't caught. Usually points to a specific driver (check the faulting module name).",
            "Identify the faulting driver from the dump file. Update or uninstall that driver."),

        [0x0000007F] = new(
            "UNEXPECTED_KERNEL_MODE_TRAP",
            "The CPU generated a trap the kernel did not expect (e.g., double fault). Often hardware-related -- overheating, bad RAM, or aggressive overclocking.",
            "Reset BIOS to defaults (remove overclocks). Check CPU temperatures. Test RAM. Update BIOS."),

        [0x0000009C] = new(
            "MACHINE_CHECK_EXCEPTION",
            "The CPU itself detected a fatal hardware error. This can indicate an overheating or failing processor, bad RAM, or power supply issues.",
            "Check CPU temps immediately. Reset overclocks. Ensure adequate cooling. Check PSU voltages."),

        [0x0000009F] = new(
            "DRIVER_POWER_STATE_FAILURE",
            "A driver failed to respond to a power state change (sleep, hibernate, or wake) in time.",
            "Update all drivers (especially network and GPU). Disable 'Allow the computer to turn off this device' for NIC/USB in Device Manager."),

        [0x000000BE] = new(
            "ATTEMPTED_WRITE_TO_READONLY_MEMORY",
            "A driver attempted to write to a memory segment marked read-only.",
            "Identify the faulting driver from the dump file and update it. Test RAM."),

        [0x000000C2] = new(
            "BAD_POOL_CALLER",
            "A kernel-mode process made an invalid pool memory request (e.g., freeing already-freed memory). Usually a driver bug.",
            "Update all third-party drivers. Use Driver Verifier to identify the culprit. Test RAM."),

        [0x000000C4] = new(
            "DRIVER_VERIFIER_DETECTED_VIOLATION",
            "Driver Verifier caught a driver doing something illegal. This is expected when Driver Verifier is enabled and catches a bad driver.",
            "Read the dump to identify the offending driver. Update or remove it. Disable Driver Verifier with 'verifier /reset' if needed."),

        [0x000000C5] = new(
            "DRIVER_CORRUPTED_EXPOOL",
            "A driver corrupted pool memory.",
            "Identify the faulting driver from the dump. Update or remove it."),

        [0x000000D1] = new(
            "DRIVER_IRQL_NOT_LESS_OR_EQUAL",
            "A driver accessed paged memory at an elevated priority level. Very common; almost always a driver bug.",
            "Identify the faulting driver from the dump file. Update, roll back, or uninstall it."),

        [0x000000D8] = new(
            "DRIVER_USED_EXCESSIVE_PTES",
            "A single driver consumed too many page table entries, exhausting a critical resource.",
            "Identify the driver from the dump. Update or remove it."),

        [0x000000EA] = new(
            "THREAD_STUCK_IN_DEVICE_DRIVER",
            "A device driver is stuck in an infinite loop, most commonly a video driver waiting for the GPU.",
            "Update GPU drivers. Check GPU temperatures. Ensure the GPU is properly seated."),

        [0x000000EF] = new(
            "CRITICAL_PROCESS_DIED",
            "A critical system process (like csrss.exe or wininit.exe) terminated unexpectedly.",
            "Run 'sfc /scannow' and 'DISM /Online /Cleanup-Image /RestoreHealth'. Scan for malware. Check disk health."),

        [0x000000F4] = new(
            "CRITICAL_OBJECT_TERMINATION",
            "A process or thread crucial to system operation unexpectedly exited.",
            "Check disk SMART health. Run 'chkdsk /f /r'. Replace SATA cables. Run 'sfc /scannow'."),

        [0x00000116] = new(
            "VIDEO_TDR_FAILURE",
            "The display driver failed to respond in time and was reset. This is a GPU driver crash.",
            "Update GPU drivers. Check for GPU overheating. Ensure adequate power supply to the GPU."),

        [0x00000119] = new(
            "VIDEO_SCHEDULER_INTERNAL_ERROR",
            "The video scheduler detected a fatal violation. Usually a GPU driver or hardware issue.",
            "Update GPU drivers. Check GPU temperatures. Test with a different GPU if possible."),

        [0x00000124] = new(
            "WHEA_UNCORRECTABLE_ERROR",
            "A hardware error was reported by the Windows Hardware Error Architecture. Could be CPU, RAM, or bus fault.",
            "Check CPU temperatures. Test RAM. Update BIOS/firmware. Reset any overclocks."),

        [0x00000133] = new(
            "DPC_WATCHDOG_VIOLATION",
            "A deferred procedure call (DPC) ran too long, indicating a hung or very slow driver.",
            "Update storage (SSD/NVMe) and network drivers. Check for firmware updates."),

        [0x00000139] = new(
            "KERNEL_SECURITY_CHECK_FAILURE",
            "The kernel detected corruption of a critical data structure. Possible driver or memory issue.",
            "Update drivers. Test RAM with MemTest86."),

        [0x0000013A] = new(
            "KERNEL_MODE_HEAP_CORRUPTION",
            "The kernel-mode heap manager detected corruption. Usually a driver bug.",
            "Update third-party kernel drivers."),

        [0x00000154] = new(
            "UNEXPECTED_STORE_EXCEPTION",
            "An unexpected exception occurred in the store component. Often disk-related.",
            "Run 'chkdsk /f /r'. Check SSD firmware. Disable fast startup in Power Options."),

        [0x00000019] = new(
            "BAD_POOL_HEADER",
            "Pool header corruption detected. Usually caused by a driver bug or bad RAM.",
            "Test RAM with MemTest86. Update all drivers."),

        [0x000001CA] = new(
            "SYNTHETIC_WATCHDOG_TIMEOUT",
            "Hyper-V watchdog timeout -- the VM or host became unresponsive.",
            "Check host health. Update Hyper-V integration services."),
    };

    public static BsodInfo GetInfo(uint code)
    {
        if (Codes.TryGetValue(code, out var info))
            return info;

        return new BsodInfo(
            $"UNKNOWN_STOP_CODE_0x{code:X8}",
            "An unrecognized blue screen error occurred.",
            $"Search online for stop code 0x{code:X8} or contact support.");
    }
}
