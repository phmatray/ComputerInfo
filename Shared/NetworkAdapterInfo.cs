namespace Shared;

public class NetworkAdapterInfo
{
    public string AdapterName { get; set; }
    public string? IPAddress { get; set; }
    public string MACAddress { get; set; }

    public override string ToString()
    {
        return $"Adapter Name: {AdapterName}\n" +
               $"IP Address: {IPAddress}\n" +
               $"MAC Address: {MACAddress}";
    }
}