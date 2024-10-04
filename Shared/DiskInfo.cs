namespace Shared;

public class DiskInfo
{
    public string DriveName { get; set; }
    public long TotalSize { get; set; }
    public long FreeSpace { get; set; }
    
    public override string ToString()
    {
        return $"Drive Name: {DriveName}\n" +
               $"Total Size: {TotalSize} bytes\n" +
               $"Free Space: {FreeSpace} bytes";
    }
}