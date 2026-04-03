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

        [0x00000019] = new(
            "BAD_POOL_HEADER",
            "Pool header corruption detected. Usually caused by a driver bug or bad RAM.",
            "Test RAM with MemTest86. Update all drivers."),

        [0x00000024] = new(
            "NTFS_FILE_SYSTEM",
            "A problem occurred within the NTFS file system driver, often due to disk corruption or a failing drive.",
            "Run 'chkdsk /f /r' on the system drive. Check disk SMART health. Replace SATA cables if applicable."),

        [0x0000002E] = new(
            "DATA_BUS_ERROR",
            "A parity error in system memory was detected, indicating a RAM hardware fault.",
            "Run Windows Memory Diagnostic or MemTest86. Reseat RAM sticks. Test individual sticks."),

        [0x00000034] = new(
            "CACHE_MANAGER",
            "The file system cache manager encountered a fatal error, often due to disk or memory issues.",
            "Run 'chkdsk /f /r'. Test RAM. Check disk SMART status."),

        [0x00000035] = new(
            "NO_MORE_IRP_STACK_LOCATIONS",
            "A driver has consumed all IRP stack locations. Usually caused by nested filter drivers.",
            "Uninstall recently added filter drivers (antivirus, backup). Check for driver conflicts."),

        [0x0000003B] = new(
            "SYSTEM_SERVICE_EXCEPTION",
            "An exception happened while executing a system service routine. Often caused by GPU drivers or antivirus software.",
            "Update GPU drivers. Disable/uninstall third-party antivirus temporarily. Run 'sfc /scannow'."),

        [0x0000003F] = new(
            "NO_MORE_SYSTEM_PTES",
            "The system ran out of page table entries. A driver is leaking kernel resources.",
            "Identify the leaking driver. Update or remove the offending driver."),

        [0x00000044] = new(
            "MULTIPLE_IRP_COMPLETE_REQUESTS",
            "A driver tried to complete an I/O request that was already completed. Always a driver bug.",
            "Identify the faulting driver from the dump. Update or remove it."),

        [0x00000050] = new(
            "PAGE_FAULT_IN_NONPAGED_AREA",
            "The system tried to access memory that should always be available but wasn't. Common causes: bad RAM, corrupt drivers, or faulty disk.",
            "Test RAM with MemTest86. Run 'chkdsk /f /r'. Update or roll back recently changed drivers."),

        [0x00000051] = new(
            "REGISTRY_ERROR",
            "A severe registry error occurred -- the hive file could not be read or is corrupted. Often caused by disk corruption.",
            "Run 'sfc /scannow' and 'chkdsk /f'. Check disk SMART health. Consider System Restore."),

        [0x00000077] = new(
            "KERNEL_STACK_INPAGE_ERROR",
            "A page of kernel data could not be read from the paging file into memory. Usually a disk problem.",
            "Run 'chkdsk /f /r'. Check disk SMART status. Test RAM with MemTest86."),

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

        [0x000000C9] = new(
            "DRIVER_VERIFIER_IOMANAGER_VIOLATION",
            "Driver Verifier detected an I/O manager violation. The driver is performing illegal I/O operations.",
            "Identify the driver from Driver Verifier output. Update or remove it."),

        [0x000000CB] = new(
            "DRIVER_LEFT_LOCKED_PAGES_IN_PROCESS",
            "A driver failed to release locked pages after an I/O operation, causing a memory leak.",
            "Identify the faulting driver from the dump. Update or remove it."),

        [0x000000CE] = new(
            "DRIVER_UNLOADED_WITHOUT_CANCELLING_PENDING_OPERATIONS",
            "A driver was unloaded while still having pending operations. Always a driver bug.",
            "Identify and update the faulting driver. This is commonly caused by network or USB drivers."),

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

        [0x000000FC] = new(
            "ATTEMPTED_EXECUTE_OF_NOEXECUTE_MEMORY",
            "A driver attempted to execute code from a memory region marked as non-executable (NX/DEP violation).",
            "Update the faulting driver. This can also be caused by malware -- run a full scan."),

        [0x000000FD] = new(
            "DIRTY_NOWRITE_PAGES_CONGESTION",
            "The system ran out of free pages due to a driver not writing dirty pages back to disk.",
            "Identify the faulting storage or filter driver. Update storage drivers and check disk health."),

        [0x00000101] = new(
            "CLOCK_WATCHDOG_TIMEOUT",
            "A processor core did not respond within the allocated time. Often caused by CPU overclocking, firmware bugs, or driver issues.",
            "Reset BIOS to defaults (remove overclocks). Update BIOS/firmware. Check CPU temperatures."),

        [0x00000109] = new(
            "CRITICAL_STRUCTURE_CORRUPTION",
            "The kernel detected that critical kernel code or data has been corrupted. Can be caused by hardware failure or a malicious driver.",
            "Test RAM. Run 'sfc /scannow'. Check for rootkits/malware. Update BIOS."),

        [0x0000010D] = new(
            "WDF_VIOLATION",
            "The Windows Driver Framework detected a violation in a WDF-based driver.",
            "Identify the faulting WDF driver from the dump. Update or remove it."),

        [0x0000010E] = new(
            "VIDEO_MEMORY_MANAGEMENT_INTERNAL",
            "The video memory manager encountered an internal error. Usually a GPU driver or hardware issue.",
            "Update GPU drivers. Check GPU VRAM health. Test with a different GPU if possible."),

        [0x00000113] = new(
            "VIDEO_DXGKRNL_FATAL_ERROR",
            "The DirectX graphics kernel subsystem detected a fatal error.",
            "Update GPU drivers. Run 'sfc /scannow'. Check for GPU hardware issues."),

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

        [0x0000012B] = new(
            "FAULTY_HARDWARE_CORRUPTED_PAGE",
            "A hardware memory error corrupted a page. Specifically indicates faulty physical RAM.",
            "Test RAM with MemTest86. Replace faulty RAM sticks. Check for BIOS memory settings issues."),

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

        [0x00000144] = new(
            "BUGCODE_USB3_DRIVER",
            "A USB 3.0 driver has caused a fatal error.",
            "Update USB/chipset drivers. Try different USB ports. Check for BIOS USB settings."),

        [0x00000154] = new(
            "UNEXPECTED_STORE_EXCEPTION",
            "An unexpected exception occurred in the store component. Often disk-related.",
            "Run 'chkdsk /f /r'. Check SSD firmware. Disable fast startup in Power Options."),

        [0x0000015F] = new(
            "CONNECTED_STANDBY_WATCHDOG_TIMEOUT",
            "The system timed out while in connected standby (Modern Standby). A driver failed to respond.",
            "Update network and storage drivers. Disable connected standby if not needed."),

        [0x00000156] = new(
            "WINSOCK_DETECTED_HUNG_CLOSESOCKET_LIVEDUMP",
            "The Winsock subsystem detected a hung close socket operation.",
            "Update network drivers. Check VPN or firewall software."),

        [0x000001C4] = new(
            "DRIVER_VERIFIER_DETECTED_VIOLATION_2",
            "Driver Verifier detected a secondary violation in a kernel driver.",
            "Identify the offending driver from Driver Verifier output. Update or uninstall it."),

        [0x000001CA] = new(
            "SYNTHETIC_WATCHDOG_TIMEOUT",
            "Hyper-V watchdog timeout -- the VM or host became unresponsive.",
            "Check host health. Update Hyper-V integration services."),

        [0x000001D3] = new(
            "HAL_IOMMU_INTERNAL_ERROR",
            "The HAL detected an internal IOMMU error. Usually a firmware or hardware issue.",
            "Update BIOS/firmware. Check for known IOMMU issues with your hardware."),

        [0x00000162] = new(
            "KERNEL_WMI_INTERNAL",
            "An internal error occurred in the kernel WMI subsystem.",
            "Run 'sfc /scannow'. Update Windows. Check for driver conflicts."),

        [0x00000171] = new(
            "CLUSTER_CSV_CLUSSVC_DISCONNECT_WATCHDOG",
            "The Cluster Shared Volume disconnected from the cluster service.",
            "Check cluster node connectivity. Review cluster event logs."),

        [0x00000196] = new(
            "LOADER_ROLLBACK_DETECTED",
            "The OS loader detected a rollback of a critical system file.",
            "Run 'sfc /scannow'. Reinstall the latest Windows Update. Check for malware."),

        [0x000001A2] = new(
            "WIN32K_ATOMIC_CHECK_FAILURE",
            "The Win32k graphics subsystem detected an atomic check failure.",
            "Update GPU drivers and Windows. Run 'sfc /scannow'."),

        [0x00000189] = new(
            "BAD_OBJECT_HEADER",
            "An object header was corrupted. Usually caused by a driver bug or memory corruption.",
            "Test RAM. Update all third-party drivers."),

        [0x0000017B] = new(
            "PROFILER_CONFIGURATION_ILLEGAL",
            "An illegal profiler configuration was detected.",
            "Remove performance profiling software. Update BIOS."),

        [0x000000E1] = new(
            "WORKER_THREAD_RETURNED_AT_BAD_IRQL",
            "A worker thread completed at an unexpected interrupt request level. Driver bug.",
            "Identify and update the faulting driver."),

        [0x000000E3] = new(
            "RESOURCE_NOT_OWNED",
            "A thread attempted to release a resource it did not own.",
            "Identify and update the faulting driver from the dump."),

        [0x000000E6] = new(
            "DRIVER_VERIFIER_DMA_VIOLATION",
            "Driver Verifier detected a DMA violation in a driver.",
            "Identify the offending driver. Update or remove it."),

        [0x0000004E] = new(
            "PFN_LIST_CORRUPT",
            "The page frame number (PFN) list has become corrupted. Usually caused by a driver bug or bad RAM.",
            "Test RAM with MemTest86. Update all drivers. Run 'sfc /scannow'."),

        [0x0000001A] = new(
            "MEMORY_MANAGEMENT",
            "A severe memory management error occurred. Can be caused by bad RAM, corrupt drivers, or disk errors.",
            "Test RAM with MemTest86. Run 'chkdsk /f /r'. Update drivers."),

        [0x000000FE] = new(
            "BUGCODE_USB_DRIVER",
            "A USB driver has caused a fatal error.",
            "Update USB and chipset drivers. Disconnect USB devices to isolate the culprit."),

        [0x00000093] = new(
            "INVALID_KERNEL_HANDLE",
            "A kernel handle was invalid or corrupted. Usually a driver bug.",
            "Update recently installed drivers. Test RAM."),

        [0x000000A0] = new(
            "INTERNAL_POWER_ERROR",
            "An internal power management error occurred.",
            "Update all drivers (especially ACPI and chipset). Check BIOS power settings."),

        [0x000000B8] = new(
            "ATTEMPTED_SWITCH_FROM_DPC",
            "A DPC routine attempted to perform an illegal thread switch.",
            "Identify and update the faulting driver."),

        [0x000000BF] = new(
            "MUTEX_ALREADY_OWNED",
            "A thread attempted to acquire a mutex it already owns.",
            "Identify and update the faulting driver from the dump."),
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
