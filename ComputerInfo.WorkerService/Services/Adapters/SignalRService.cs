using ComputerInfo.WorkerService.Configuration;
using ComputerInfo.WorkerService.Services.Ports;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Shared;

namespace ComputerInfo.WorkerService.Services.Adapters;

public class SignalRService(
    IOptions<SignalRSettings> options,
    ILogger<SignalRService> logger)
    : ISignalRService
{
    private readonly HubConnection _connection = new HubConnectionBuilder()
        .WithUrl(options.Value.HubUrl)
        .Build();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _connection.StartAsync(cancellationToken);
            logger.LogInformation("SignalR client connected.");
        }
        catch (Exception ex)
        {
            logger.LogError("Error starting SignalR connection: {Message}", ex.Message);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _connection.StopAsync(cancellationToken);
    }

    public async Task SendMachineInfoAsync(MachineInfo machineName, CancellationToken cancellationToken)
    {
        try
        {
            await _connection.InvokeAsync("SendMachineInfo", machineName, cancellationToken: cancellationToken);
            logger.LogInformation("Sent machine info: {MachineName}", machineName);
        }
        catch (Exception ex)
        {
            logger.LogError("Error during SignalR communication: {Message}", ex.Message);
            throw;
        }
    }
}