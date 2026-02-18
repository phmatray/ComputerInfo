using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ComputerInfo.WorkerService.Services.Ports;
using Shared;

namespace ComputerInfo.WorkerService.Services.Adapters;

public class WindowsMachineInfoProvider : IMachineInfoProvider
{
    private readonly PerformanceCounter _cpuCounter;
    
    public WindowsMachineInfoProvider()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _cpuCounter.NextValue(); // First call always returns 0
    }
    
    public MachineInfo GetMachineInfo()
    {
        var totalMemory = GetTotalPhysicalMemory();
        var availableMemory = GetAvailableMemory();
        var memoryUsagePercentage = totalMemory > 0 
            ? ((totalMemory - availableMemory) / (double)totalMemory) * 100 
            : 0;
        
        var machineInfo = new MachineInfo
        {
            MachineName = Environment.MachineName,
            OperatingSystem = RuntimeInformation.OSDescription,
            OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            CPUArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            TotalPhysicalMemory = totalMemory,
            AvailableMemory = availableMemory,
            CpuUsagePercentage = GetCpuUsagePercentage(),
            MemoryUsagePercentage = memoryUsagePercentage,
            DiskDrives = GetDiskDrives(),
            NetworkAdapters = GetNetworkAdapters(),
            UpTime = GetUpTime()
        };

        return machineInfo;
    }

    private long GetTotalPhysicalMemory()
    {
        // Use WMI to get total physical memory
        using var searcher = new System.Management.ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
        foreach (var obj in searcher.Get())
        {
            if (obj["TotalPhysicalMemory"] != null)
            {
                return Convert.ToInt64(obj["TotalPhysicalMemory"]);
            }
        }
        
        // Fallback to approximate value if WMI fails
        return 0;
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
    
    private double GetCpuUsagePercentage()
    {
        try
        {
            return Math.Round(_cpuCounter.NextValue(), 2);
        }
        catch
        {
            return 0;
        }
    }
}