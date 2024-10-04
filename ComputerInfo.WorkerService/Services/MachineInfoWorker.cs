using ComputerInfo.WorkerService.Services.Adapters;
using ComputerInfo.WorkerService.Services.Ports;
using Shared;

namespace ComputerInfo.WorkerService.Services;

public class MachineInfoWorker(
    ILogger<MachineInfoWorker> logger,
    ISignalRService signalRService,
    IMachineInfoProvider machineInfoProvider)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await signalRService.StartAsync(stoppingToken);
        }
        catch
        {
            return;
        }

        try
        {
            MachineInfo machineInfo = machineInfoProvider.GetMachineInfo();
            await signalRService.SendMachineInfoAsync(machineInfo, stoppingToken);
        }
        catch
        {
            // Exception already logged in SendMachineInfoAsync
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var machineInfo = machineInfoProvider.GetMachineInfo();
                await signalRService.SendMachineInfoAsync(machineInfo, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("Error during periodic execution: {Message}", ex.Message);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        await signalRService.StopAsync(stoppingToken);
    }
}