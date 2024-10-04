using ComputerInfo.WorkerService.Services.Adapters;
using Shared;

namespace ComputerInfo.WorkerService.Services.Ports;

public interface ISignalRService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task SendMachineInfoAsync(MachineInfo machineName, CancellationToken cancellationToken);
}