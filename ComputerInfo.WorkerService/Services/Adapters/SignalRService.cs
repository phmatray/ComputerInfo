using ComputerInfo.WorkerService.Configuration;
using ComputerInfo.WorkerService.Services.Ports;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace ComputerInfo.WorkerService.Services.Adapters;

public class SignalRService : ISignalRService
{
    private readonly HubConnection _connection;
    private readonly ILogger<SignalRService> _logger;

    public SignalRService(IOptions<SignalRSettings> options, ILogger<SignalRService> logger)
    {
        _logger = logger;
        _connection = new HubConnectionBuilder()
            .WithUrl(options.Value.HubUrl)
            .Build();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _connection.StartAsync(cancellationToken);
            _logger.LogInformation("SignalR client connected.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error starting SignalR connection: {Message}", ex.Message);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _connection.StopAsync(cancellationToken);
    }

    public async Task SendMachineInfoAsync(string machineName, CancellationToken cancellationToken)
    {
        try
        {
            await _connection.InvokeAsync("SendMachineInfo", machineName, cancellationToken: cancellationToken);
            _logger.LogInformation("Sent machine info: {MachineName}", machineName);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during SignalR communication: {Message}", ex.Message);
            throw;
        }
    }
}