using Microsoft.AspNetCore.SignalR.Client;

namespace ComputerInfo.WorkerService;

public class Worker(ILogger<Worker> logger)
    : BackgroundService
{
    private HubConnection? _connection;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initialize the SignalR connection
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5042/machinehub")
            .Build();
        
        // Start the connection
        try
        {
            await _connection.StartAsync(stoppingToken);
            logger.LogInformation("SignalR client connected.");
        }
        catch (Exception ex)
        {
            logger.LogError("Error starting SignalR connection: {Message}", ex.Message);
            return;
        }
        
        // Immediately send the machine name after connection is established
        try
        {
            string machineName = Environment.MachineName;
            await _connection.InvokeAsync("SendMachineInfo", machineName, cancellationToken: stoppingToken);
            logger.LogInformation("Sent machine info: {MachineName}", machineName);
        }
        catch (Exception ex)
        {
            logger.LogError("Error during SignalR communication: {Message}", ex.Message);
        }
        
        // Periodically send machine info every 1 minute
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for 1 minute before sending again
                await Task.Delay(60000, stoppingToken);

                // Send machine name to the SignalR hub again
                string machineName = Environment.MachineName;
                await _connection.InvokeAsync("SendMachineInfo", machineName, cancellationToken: stoppingToken);
                logger.LogInformation("Sent machine info: {MachineName}", machineName);
            }
            catch (Exception ex)
            {
                logger.LogError("Error during SignalR communication: {Message}", ex.Message);
            }
        }

        // Stop the SignalR connection when the service is stopped
        await _connection.StopAsync(stoppingToken);
    }
}