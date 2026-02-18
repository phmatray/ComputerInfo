namespace Shared.Helpers;

public static class FormatHelper
{
    private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
    
    public static string FormatBytes(long bytes)
    {
        if (bytes == 0)
            return "0 B";
        
        var isNegative = bytes < 0;
        bytes = Math.Abs(bytes);
        
        var mag = (int)Math.Log(bytes, 1024);
        var adjustedSize = (decimal)bytes / (1L << (mag * 10));
        
        if (Math.Round(adjustedSize, 2) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }
        
        var result = string.Format("{0:n2} {1}", adjustedSize, SizeSuffixes[mag]);
        return isNegative ? "-" + result : result;
    }
    
    public static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
        {
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        }
        else if (uptime.TotalHours >= 1)
        {
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m {uptime.Seconds}s";
        }
        else if (uptime.TotalMinutes >= 1)
        {
            return $"{(int)uptime.TotalMinutes}m {uptime.Seconds}s";
        }
        else
        {
            return $"{(int)uptime.TotalSeconds}s";
        }
    }
    
    public static string FormatPercentage(double percentage)
    {
        return $"{percentage:F1}%";
    }
}