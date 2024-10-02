using Microsoft.AspNetCore.SignalR;

namespace ComputerInfo.BlazorApp;

public class MachineHub(ILogger<MachineHub> logger) : Hub
{
    // This method will be called by the Windows service to send machine info
    public async Task SendMachineInfo(string machineName)
    {
        // Process the machine name (e.g., log it, save it to a database, etc.)
        logger.LogInformation("Received machine info: {MachineName}", machineName);

        // Optionally broadcast the machine info to all connected clients
        await Clients.All.SendAsync("ReceiveMachineInfo", machineName);
    }
}