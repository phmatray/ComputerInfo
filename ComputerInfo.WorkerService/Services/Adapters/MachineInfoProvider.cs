using ComputerInfo.WorkerService.Services.Ports;

namespace ComputerInfo.WorkerService.Services.Adapters;

public class MachineInfoProvider : IMachineInfoProvider
{
    public string GetMachineName()
    {
        return Environment.MachineName;
    }
}