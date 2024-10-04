using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ComputerInfo.WorkerService.Services.Ports;
using Shared;

namespace ComputerInfo.WorkerService.Services.Adapters;

public class WindowsMachineInfoProvider : IMachineInfoProvider
{
    public MachineInfo GetMachineInfo()
    {
        var machineInfo = new MachineInfo
        {
            MachineName = Environment.MachineName,
            OperatingSystem = RuntimeInformation.OSDescription,
            OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            CPUArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            TotalPhysicalMemory = GetTotalPhysicalMemory(),
            AvailableMemory = GetAvailableMemory(),
            DiskDrives = GetDiskDrives(),
            NetworkAdapters = GetNetworkAdapters(),
            UpTime = GetUpTime()
        };

        return machineInfo;
    }

    private long GetTotalPhysicalMemory()
    {
        // Use PerformanceCounter or WMI
        var pc = new PerformanceCounter("Memory", "Available Bytes");
        return pc.RawValue;
    }

    private long GetAvailableMemory()
    {
        // Use PerformanceCounter or WMI
        var pc = new PerformanceCounter("Memory", "Available Bytes");
        return pc.RawValue;
    }

    // Disk and network logic can remain similar across platforms
    private List<DiskInfo> GetDiskDrives()
    {
        return DriveInfo.GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
            .Select(d => new DiskInfo
            {
                DriveName = d.Name,
                TotalSize = d.TotalSize,
                FreeSpace = d.AvailableFreeSpace
            })
            .ToList();
    }

    private List<NetworkAdapterInfo> GetNetworkAdapters()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
            .Select(ni => new NetworkAdapterInfo
            {
                AdapterName = ni.Name,
                IPAddress = ni.GetIPProperties().UnicastAddresses
                    .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ua => ua.Address.ToString())
                    .FirstOrDefault(),
                MACAddress = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes())
            })
            .ToList();
    }

    private TimeSpan GetUpTime()
    {
        return TimeSpan.FromMilliseconds(Environment.TickCount64);
    }
}