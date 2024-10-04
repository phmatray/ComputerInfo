using ComputerInfo.WorkerService.Services.Adapters;
using Shared;

namespace ComputerInfo.WorkerService.Services.Ports;

public interface IMachineInfoProvider
{
    MachineInfo GetMachineInfo();
}