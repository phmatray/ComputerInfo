using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ComputerInfo.WorkerService.Services.Ports;
using Shared;

namespace ComputerInfo.WorkerService.Services.Adapters;

public class MacOsMachineInfoProvider : IMachineInfoProvider
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
        return GetSysctlValue("hw.memsize");
    }

    private long GetAvailableMemory()
    {
        var output = RunBashCommand("vm_stat");
        var lines = output.Split('\n');
        foreach (var line in lines)
        {
            if (line.StartsWith("Pages free:"))
            {
                var parts = line.Split(':');
                var str = parts[1].Trim().TrimEnd('.');
                var pagesFree = long.Parse(str);
                return pagesFree * 4096; // Pages are typically 4KB
            }
        }
        return 0;
    }

    private long GetSysctlValue(string key)
    {
        var output = RunBashCommand($"sysctl {key}");
        var parts = output.Split(": ");
        var str = parts[1].Trim().TrimEnd('.');
        return long.Parse(str);
    }

    private string RunBashCommand(string command)
    {
        var escapedArgs = command.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
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
}