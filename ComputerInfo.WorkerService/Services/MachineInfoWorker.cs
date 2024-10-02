using ComputerInfo.WorkerService.Services.Ports;

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
            string machineName = machineInfoProvider.GetMachineName();
            await signalRService.SendMachineInfoAsync(machineName, stoppingToken);
        }
        catch
        {
            // Exception already logged in SendMachineInfoAsync
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string machineName = machineInfoProvider.GetMachineName();
                await signalRService.SendMachineInfoAsync(machineName, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("Error during periodic execution: {Message}", ex.Message);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        await signalRService.StopAsync(stoppingToken);
    }
}