using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ComputerInfo.WorkerService.Services.Ports;
using Shared;

namespace ComputerInfo.WorkerService.Services.Adapters;

public class LinuxMachineInfoProvider : IMachineInfoProvider
{
    private double _lastCpuUsage = 0;
    private DateTime _lastCpuCheck = DateTime.MinValue;
    
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
            MemoryUsagePercentage = Math.Round(memoryUsagePercentage, 2),
            DiskDrives = GetDiskDrives(),
            NetworkAdapters = GetNetworkAdapters(),
            UpTime = GetUpTime()
        };

        return machineInfo;
    }

    private long GetTotalPhysicalMemory()
    {
        var lines = File.ReadAllLines("/proc/meminfo");
        foreach (var line in lines)
        {
            if (line.StartsWith("MemTotal:"))
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                var str = parts[1].Trim().TrimEnd('.');
                return long.Parse(str) * 1024;
            }
        }
        return 0;
    }

    private long GetAvailableMemory()
    {
        var lines = File.ReadAllLines("/proc/meminfo");
        foreach (var line in lines)
        {
            if (line.StartsWith("MemAvailable:"))
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                var str = parts[1].Trim().TrimEnd('.');
                return long.Parse(str) * 1024;
            }
        }
        return 0;
    }

    // Disk and network methods can be similar to other platforms
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
            // Cache CPU usage for 1 second to avoid too frequent calls
            if ((DateTime.Now - _lastCpuCheck).TotalSeconds < 1)
            {
                return _lastCpuUsage;
            }
            
            // Read CPU stats from /proc/stat
            var lines = File.ReadAllLines("/proc/stat");
            var cpuLine = lines.FirstOrDefault(l => l.StartsWith("cpu "));
            if (cpuLine != null)
            {
                var parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 5)
                {
                    var idle = long.Parse(parts[4]);
                    var total = parts.Skip(1).Take(7).Sum(p => long.Parse(p));
                    
                    // For a more accurate reading, we'd need to track previous values
                    // This is a simplified version that gives an approximation
                    var usage = 100.0 - (idle * 100.0 / total);
                    _lastCpuUsage = Math.Round(Math.Min(100, Math.Max(0, usage)), 2);
                    _lastCpuCheck = DateTime.Now;
                    return _lastCpuUsage;
                }
            }
        }
        catch
        {
            // Ignore errors and return last known value
        }
        
        return _lastCpuUsage;
    }
}