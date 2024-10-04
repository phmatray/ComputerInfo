namespace Shared;

public class MachineInfo
{
    public string MachineName { get; set; }
    public string OperatingSystem { get; set; }
    public string OSArchitecture { get; set; }
    public int ProcessorCount { get; set; }
    public string CPUArchitecture { get; set; }
    public long TotalPhysicalMemory { get; set; }
    public long AvailableMemory { get; set; }
    public List<DiskInfo> DiskDrives { get; set; } = [];
    public List<NetworkAdapterInfo> NetworkAdapters { get; set; } = [];
    public TimeSpan UpTime { get; set; }

    public override string ToString()
    {
        return $"Machine Name: {MachineName}\n" +
               $"Operating System: {OperatingSystem}\n" +
               $"OS Architecture: {OSArchitecture}\n" +
               $"Processor Count: {ProcessorCount}\n" +
               $"CPU Architecture: {CPUArchitecture}\n" +
               $"Total Physical Memory: {TotalPhysicalMemory} bytes\n" +
               $"Available Memory: {AvailableMemory} bytes\n" +
               $"Disk Drives: {string.Join(", ", DiskDrives.Select(d => d.DriveName))}\n" +
               $"Network Adapters: {string.Join(", ", NetworkAdapters.Select(n => n.AdapterName))}\n" +
               $"Up Time: {UpTime}";
    }
}