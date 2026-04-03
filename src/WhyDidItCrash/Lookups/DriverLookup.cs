namespace WhyDidItCrash.Lookups;

public static class DriverLookup
{
    public record DriverInfo(string Component, string Explanation);

    private static readonly Dictionary<string, DriverInfo> Drivers = new(StringComparer.OrdinalIgnoreCase)
    {
        // Graphics drivers
        ["nvlddmkm.sys"] = new("NVIDIA Display Driver", "NVIDIA GPU driver crash. Update or clean-install GPU drivers using DDU."),
        ["nvkflt.sys"] = new("NVIDIA Display Driver", "NVIDIA GPU kernel filter crash. Update GPU drivers."),
        ["nvoclock.sys"] = new("NVIDIA Overclock Driver", "NVIDIA overclocking driver crash. Remove overclock software or update drivers."),
        ["atikmdag.sys"] = new("AMD/ATI Display Driver", "AMD GPU driver crash. Update or clean-install GPU drivers using DDU."),
        ["atikmpag.sys"] = new("AMD/ATI Display Driver", "AMD GPU driver crash. Update GPU drivers."),
        ["amdppm.sys"] = new("AMD Processor Power Management", "AMD CPU power management driver issue. Update chipset drivers."),
        ["igdkmd64.sys"] = new("Intel Graphics Driver", "Intel integrated GPU driver crash. Update via Intel Driver Support Assistant."),
        ["igdkmd32.sys"] = new("Intel Graphics Driver (32-bit)", "Intel integrated GPU driver crash. Update via Intel Driver Support Assistant."),
        ["dxgkrnl.sys"] = new("DirectX Graphics Kernel", "GPU subsystem crash. Update your GPU drivers."),
        ["dxgmms1.sys"] = new("DirectX Graphics Memory Manager", "GPU memory management crash. Check GPU drivers and VRAM health."),
        ["dxgmms2.sys"] = new("DirectX Graphics Memory Manager v2", "GPU memory management crash. Update GPU drivers."),

        // Network drivers
        ["ndis.sys"] = new("Network Driver Interface", "Network driver issue. Update network adapter drivers."),
        ["tcpip.sys"] = new("TCP/IP Stack", "Windows networking stack issue. Run 'netsh winsock reset'. Update network drivers."),
        ["afd.sys"] = new("Ancillary Function Driver (Winsock)", "Network socket layer issue. May be caused by network software or VPN."),
        ["nwifi.sys"] = new("Native WiFi Driver", "WiFi driver crash. Update wireless adapter drivers."),
        ["rtwlane.sys"] = new("Realtek Wireless Driver", "Realtek WiFi driver crash. Update from Realtek or your laptop manufacturer."),
        ["rtwlane6.sys"] = new("Realtek Wireless Driver v6", "Realtek WiFi driver crash. Update from Realtek or your laptop manufacturer."),
        ["rt640x64.sys"] = new("Realtek Ethernet Driver", "Realtek network driver crash. Update from Realtek or your PC manufacturer."),
        ["e1d65x64.sys"] = new("Intel Ethernet Driver", "Intel network driver crash. Update via Intel Driver Support Assistant."),
        ["e1i65x64.sys"] = new("Intel Ethernet Driver", "Intel network driver crash. Update via Intel Driver Support Assistant."),
        ["Netwtw10.sys"] = new("Intel WiFi 6 Driver", "Intel WiFi driver crash. Update via Intel Driver Support Assistant."),
        ["Netwtw08.sys"] = new("Intel WiFi Driver", "Intel WiFi driver crash. Update via Intel Driver Support Assistant."),
        ["mrvlpcie8897.sys"] = new("Marvell WiFi Driver", "Marvell wireless driver crash. Update from device manufacturer."),

        // Storage drivers
        ["storport.sys"] = new("Storage Port Driver", "Storage controller issue. Update storage/chipset drivers."),
        ["stornvme.sys"] = new("NVMe Storage Driver", "NVMe SSD driver issue. Update NVMe/chipset drivers and SSD firmware."),
        ["iaStorA.sys"] = new("Intel Rapid Storage Technology", "Intel SATA/RAID driver issue. Update Intel RST drivers."),
        ["iaStorAVC.sys"] = new("Intel RST Virtual Controller", "Intel storage driver crash. Update Intel RST."),
        ["iaStorE.sys"] = new("Intel RST Enterprise", "Intel enterprise storage driver crash. Update Intel RST."),
        ["CLASSPNP.SYS"] = new("SCSI Class Driver", "Storage device communication failure. Check disk connections and health."),
        ["volsnap.sys"] = new("Volume Shadow Copy", "Shadow copy/backup service issue. Check disk health."),
        ["disk.sys"] = new("Disk Driver", "Low-level disk driver crash. Check disk connections and SMART status."),
        ["partmgr.sys"] = new("Partition Manager", "Disk partition manager issue. Check disk health. Run 'chkdsk /f /r'."),
        ["spaceport.sys"] = new("Storage Spaces Driver", "Windows Storage Spaces driver crash. Check pool health."),
        ["refs.sys"] = new("ReFS File System", "Resilient File System driver crash. Check disk health."),
        ["storahci.sys"] = new("AHCI Storage Driver", "AHCI controller driver issue. Update chipset/SATA drivers."),

        // USB drivers
        ["USBPORT.sys"] = new("USB Port Driver", "USB controller issue. Update USB/chipset drivers. Try different USB ports."),
        ["USBHUB.sys"] = new("USB Hub Driver", "USB hub issue. Disconnect USB devices one at a time to isolate."),
        ["USBXHCI.sys"] = new("USB 3.0 (xHCI) Driver", "USB 3.0 controller issue. Update chipset drivers."),
        ["USBHUB3.sys"] = new("USB 3.0 Hub Driver", "USB 3.0 hub issue. Update chipset drivers. Check connected devices."),
        ["ucx01000.sys"] = new("USB Host Controller Extension", "USB host controller crash. Update chipset drivers."),

        // Audio drivers
        ["HdAudio.sys"] = new("HD Audio Bus Driver", "High Definition Audio bus driver crash. Update audio drivers."),
        ["RTKVHD64.sys"] = new("Realtek HD Audio Driver", "Realtek audio driver crash. Update from Realtek or PC manufacturer."),
        ["ksthunk.sys"] = new("Kernel Streaming Thunk", "Audio/video streaming kernel layer crash. Update audio/video drivers."),
        ["portcls.sys"] = new("Port Class Audio Driver", "Audio port driver crash. Update audio drivers."),

        // File system / filter drivers
        ["ntfs.sys"] = new("NTFS File System", "File system corruption detected. Run 'chkdsk /f /r'. Check disk health."),
        ["fltMgr.sys"] = new("File System Filter Manager", "Often caused by antivirus or backup filter drivers. Check third-party security software."),

        // Windows core
        ["win32kfull.sys"] = new("Win32 Kernel Subsystem", "Windows display/UI subsystem crash. Update Windows and GPU drivers."),
        ["win32kbase.sys"] = new("Win32 Kernel Base", "Windows display/UI subsystem crash. Run Windows Update."),
        ["Wdf01000.sys"] = new("Windows Driver Framework", "A WDF-based driver crashed. Check the full call stack for the actual driver."),
        ["CI.dll"] = new("Code Integrity", "Windows code integrity check failed. May indicate corrupted system files or secure boot issues."),
        ["tm.sys"] = new("Transaction Manager", "Windows transaction manager crash. Check disk health."),

        // Chipset / platform
        ["pci.sys"] = new("PCI Bus Driver", "PCI bus driver crash. Update BIOS and chipset drivers. Check hardware seating."),
        ["acpi.sys"] = new("ACPI Driver", "Power management (ACPI) driver crash. Update BIOS/firmware and chipset drivers."),
        ["intelppm.sys"] = new("Intel Processor Power Management", "Intel CPU power management issue. Update chipset drivers and BIOS."),
        ["GenuineIntel.sys"] = new("Intel Microcode Update", "Intel microcode driver issue. Update BIOS for latest CPU microcode."),
        ["AmdPPM.sys"] = new("AMD Power Management", "AMD CPU power management issue. Update chipset drivers."),

        // Bluetooth
        ["bthport.sys"] = new("Bluetooth Port Driver", "Bluetooth stack crash. Update Bluetooth drivers."),
        ["BthEnum.sys"] = new("Bluetooth Enumerator", "Bluetooth device enumeration crash. Update Bluetooth drivers."),
        ["BTHUSB.sys"] = new("Bluetooth USB Driver", "Bluetooth USB adapter crash. Update Bluetooth and USB drivers."),
        ["bthhfenum.sys"] = new("Bluetooth Hands-Free Enumerator", "Bluetooth audio device crash. Update Bluetooth drivers."),

        // Antivirus / security
        ["klif.sys"] = new("Kaspersky Lab Driver", "Kaspersky antivirus driver crash. Update or reinstall Kaspersky."),
        ["tmtdi.sys"] = new("Trend Micro Driver", "Trend Micro antivirus driver crash. Update or reinstall Trend Micro."),
        ["aswSP.sys"] = new("Avast Driver", "Avast antivirus driver crash. Update or reinstall Avast."),
        ["avgSP.sys"] = new("AVG Driver", "AVG antivirus driver crash. Update or reinstall AVG."),
        ["mfehidk.sys"] = new("McAfee Driver", "McAfee antivirus driver crash. Update or reinstall McAfee."),
        ["bdvedisk.sys"] = new("Bitdefender Driver", "Bitdefender driver crash. Update or reinstall Bitdefender."),
        ["WdFilter.sys"] = new("Windows Defender Filter Driver", "Windows Defender filter driver crash. Run 'sfc /scannow'. Update Windows."),
        ["SbieDrv.sys"] = new("Sandboxie Driver", "Sandboxie sandbox driver crash. Update or reinstall Sandboxie."),
        ["ehdrv.sys"] = new("ESET Driver", "ESET antivirus driver crash. Update or reinstall ESET."),
        ["eamonm.sys"] = new("ESET Real-Time Monitor", "ESET real-time protection driver crash. Update ESET."),
        ["epfwwfp.sys"] = new("ESET Firewall Driver", "ESET firewall driver crash. Update or reinstall ESET."),
        ["aswMonFlt.sys"] = new("Avast Monitoring Filter", "Avast file system monitor crash. Update or reinstall Avast."),
        ["aswArPot.sys"] = new("Avast Anti-Rootkit", "Avast anti-rootkit driver crash. Update or reinstall Avast."),

        // VPN / networking software
        ["tap0901.sys"] = new("OpenVPN TAP Adapter", "OpenVPN virtual network adapter crash. Update OpenVPN."),
        ["wintun.sys"] = new("WireGuard Tunnel Driver", "WireGuard tunnel driver crash. Update WireGuard or the VPN client."),
        ["NordLynx.sys"] = new("NordVPN NordLynx Driver", "NordVPN tunnel driver crash. Update NordVPN."),

        // Gaming / anti-cheat
        ["EasyAntiCheat.sys"] = new("Easy Anti-Cheat", "EasyAntiCheat driver crash. Verify game files. Reinstall EAC."),
        ["BEDaisy.sys"] = new("BattlEye Anti-Cheat", "BattlEye driver crash. Verify game files. Reinstall BattlEye."),
        ["vgk.sys"] = new("Riot Vanguard Anti-Cheat", "Riot Vanguard driver crash. Reinstall Vanguard via the Riot client."),
        ["xhunter1.sys"] = new("XIGNCODE3 Anti-Cheat", "XIGNCODE3 driver crash. Reinstall the game."),

        // Peripheral drivers
        ["RzDev.sys"] = new("Razer Device Driver", "Razer peripheral driver crash. Update Razer Synapse."),

        // Virtualization
        ["vmswitch.sys"] = new("Hyper-V Virtual Switch", "Hyper-V network switch issue. Check Hyper-V configuration."),
        ["vmmouse.sys"] = new("VMware Mouse Driver", "VMware Tools driver issue. Update VMware Tools."),
        ["VBoxGuest.sys"] = new("VirtualBox Guest Additions", "VirtualBox driver issue. Update Guest Additions."),
        ["hvix64.sys"] = new("Hyper-V Hypervisor", "Hyper-V hypervisor crash. Update Windows and check virtualization settings."),
        ["winhvr.sys"] = new("Windows Hypervisor Platform", "Windows hypervisor crash. Update Windows. Check BIOS virtualization settings."),
        ["vmbus.sys"] = new("Hyper-V VMBus", "Hyper-V virtual machine bus crash. Update integration services."),
    };

    public static DriverInfo? GetInfo(string driverName)
    {
        if (Drivers.TryGetValue(driverName, out var info))
            return info;

        return null;
    }
}
