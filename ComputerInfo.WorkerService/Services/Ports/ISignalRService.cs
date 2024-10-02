namespace ComputerInfo.WorkerService.Services.Ports;

public interface ISignalRService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task SendMachineInfoAsync(string machineName, CancellationToken cancellationToken);
}