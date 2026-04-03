namespace WhyDidItCrash.Lookups;

public static class DriverLookup
{
    public record DriverInfo(string Component, string Explanation);

    private static readonly Dictionary<string, DriverInfo> Drivers = new(StringComparer.OrdinalIgnoreCase)
    {
        // Graphics drivers
        ["nvlddmkm.sys"] = new("NVIDIA Display Driver", "NVIDIA GPU driver crash. Update or clean-install GPU drivers using DDU."),
        ["nvkflt.sys"] = new("NVIDIA Display Driver", "NVIDIA GPU kernel filter crash. Update GPU drivers."),
        ["atikmdag.sys"] = new("AMD/ATI Display Driver", "AMD GPU driver crash. Update or clean-install GPU drivers using DDU."),
        ["atikmpag.sys"] = new("AMD/ATI Display Driver", "AMD GPU driver crash. Update GPU drivers."),
        ["igdkmd64.sys"] = new("Intel Graphics Driver", "Intel integrated GPU driver crash. Update via Intel Driver Support Assistant."),
        ["dxgkrnl.sys"] = new("DirectX Graphics Kernel", "GPU subsystem crash. Update your GPU drivers."),
        ["dxgmms1.sys"] = new("DirectX Graphics Memory Manager", "GPU memory management crash. Check GPU drivers and VRAM health."),

        // Network drivers
        ["ndis.sys"] = new("Network Driver Interface", "Network driver issue. Update network adapter drivers."),
        ["tcpip.sys"] = new("TCP/IP Stack", "Windows networking stack issue. Run 'netsh winsock reset'. Update network drivers."),
        ["afd.sys"] = new("Ancillary Function Driver (Winsock)", "Network socket layer issue. May be caused by network software or VPN."),
        ["nwifi.sys"] = new("Native WiFi Driver", "WiFi driver crash. Update wireless adapter drivers."),
        ["rtwlane.sys"] = new("Realtek Wireless Driver", "Realtek WiFi driver crash. Update from Realtek or your laptop manufacturer."),
        ["rt640x64.sys"] = new("Realtek Ethernet Driver", "Realtek network driver crash. Update from Realtek or your PC manufacturer."),
        ["e1d65x64.sys"] = new("Intel Ethernet Driver", "Intel network driver crash. Update via Intel Driver Support Assistant."),

        // Storage drivers
        ["storport.sys"] = new("Storage Port Driver", "Storage controller issue. Update storage/chipset drivers."),
        ["stornvme.sys"] = new("NVMe Storage Driver", "NVMe SSD driver issue. Update NVMe/chipset drivers and SSD firmware."),
        ["iaStorA.sys"] = new("Intel Rapid Storage Technology", "Intel SATA/RAID driver issue. Update Intel RST drivers."),
        ["iaStorAVC.sys"] = new("Intel RST Virtual Controller", "Intel storage driver crash. Update Intel RST."),
        ["CLASSPNP.SYS"] = new("SCSI Class Driver", "Storage device communication failure. Check disk connections and health."),
        ["volsnap.sys"] = new("Volume Shadow Copy", "Shadow copy/backup service issue. Check disk health."),

        // USB drivers
        ["USBPORT.sys"] = new("USB Port Driver", "USB controller issue. Update USB/chipset drivers. Try different USB ports."),
        ["USBHUB.sys"] = new("USB Hub Driver", "USB hub issue. Disconnect USB devices one at a time to isolate."),
        ["USBXHCI.sys"] = new("USB 3.0 (xHCI) Driver", "USB 3.0 controller issue. Update chipset drivers."),

        // File system / filter drivers
        ["ntfs.sys"] = new("NTFS File System", "File system corruption detected. Run 'chkdsk /f /r'. Check disk health."),
        ["fltMgr.sys"] = new("File System Filter Manager", "Often caused by antivirus or backup filter drivers. Check third-party security software."),

        // Windows core
        ["win32kfull.sys"] = new("Win32 Kernel Subsystem", "Windows display/UI subsystem crash. Update Windows and GPU drivers."),
        ["win32kbase.sys"] = new("Win32 Kernel Base", "Windows display/UI subsystem crash. Run Windows Update."),
        ["Wdf01000.sys"] = new("Windows Driver Framework", "A WDF-based driver crashed. Check the full call stack for the actual driver."),

        // Antivirus / security
        ["klif.sys"] = new("Kaspersky Lab Driver", "Kaspersky antivirus driver crash. Update or reinstall Kaspersky."),
        ["tmtdi.sys"] = new("Trend Micro Driver", "Trend Micro antivirus driver crash. Update or reinstall Trend Micro."),
        ["aswSP.sys"] = new("Avast Driver", "Avast antivirus driver crash. Update or reinstall Avast."),
        ["avgSP.sys"] = new("AVG Driver", "AVG antivirus driver crash. Update or reinstall AVG."),
        ["mfehidk.sys"] = new("McAfee Driver", "McAfee antivirus driver crash. Update or reinstall McAfee."),
        ["bdvedisk.sys"] = new("Bitdefender Driver", "Bitdefender driver crash. Update or reinstall Bitdefender."),

        // VPN / virtualization
        ["vmswitch.sys"] = new("Hyper-V Virtual Switch", "Hyper-V network switch issue. Check Hyper-V configuration."),
        ["vmmouse.sys"] = new("VMware Mouse Driver", "VMware Tools driver issue. Update VMware Tools."),
        ["VBoxGuest.sys"] = new("VirtualBox Guest Additions", "VirtualBox driver issue. Update Guest Additions."),
    };

    public static DriverInfo? GetInfo(string driverName)
    {
        if (Drivers.TryGetValue(driverName, out var info))
            return info;

        return null;
    }
}
