using System.Net.Sockets;
using ComputerInfo.WorkerService.Configuration;
using ComputerInfo.WorkerService.Services.Ports;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Shared;

namespace ComputerInfo.WorkerService.Services.Adapters;

public class SignalRService(
    IOptions<SignalRSettings> options,
    ILogger<SignalRService> logger)
    : ISignalRService, IDisposable
{
    private readonly HubConnection _connection = new HubConnectionBuilder()
        .WithUrl(options.Value.HubUrl)
        .WithAutomaticReconnect()
        .Build();
    
    private bool _isConnected = false;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Set up reconnection handlers
        _connection.Reconnecting += (error) =>
        {
            logger.LogWarning("SignalR connection lost. Attempting to reconnect...");
            _isConnected = false;
            return Task.CompletedTask;
        };
        
        _connection.Reconnected += (connectionId) =>
        {
            logger.LogInformation("SignalR connection restored. Connection ID: {ConnectionId}", connectionId);
            _isConnected = true;
            return Task.CompletedTask;
        };
        
        _connection.Closed += (error) =>
        {
            logger.LogError("SignalR connection closed: {Error}", error?.Message ?? "Unknown error");
            _isConnected = false;
            return Task.CompletedTask;
        };
        
        // Try to connect with retry logic
        var retryCount = 0;
        const int maxRetries = 5;
        
        while (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Attempting to connect to SignalR hub at {HubUrl} (attempt {Attempt}/{MaxRetries})", 
                    options.Value.HubUrl, retryCount + 1, maxRetries);
                    
                await _connection.StartAsync(cancellationToken);
                _isConnected = true;
                logger.LogInformation("SignalR client connected successfully.");
                return;
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                retryCount++;
                if (retryCount < maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount)); // Exponential backoff
                    logger.LogWarning("Failed to connect to SignalR hub. Retrying in {Delay} seconds...", delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    logger.LogError("Failed to connect to SignalR hub after {MaxRetries} attempts. Make sure the Blazor app is running on {HubUrl}", 
                        maxRetries, options.Value.HubUrl);
                    // Don't throw - allow the service to continue running and try to reconnect later
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error starting SignalR connection");
                throw;
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _connection.StopAsync(cancellationToken);
    }

    public async Task SendMachineInfoAsync(MachineInfo machineInfo, CancellationToken cancellationToken)
    {
        // Ensure we're connected before trying to send
        if (!_isConnected)
        {
            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (!_isConnected && _connection.State == HubConnectionState.Disconnected)
                {
                    logger.LogInformation("SignalR not connected. Attempting to reconnect...");
                    try
                    {
                        await _connection.StartAsync(cancellationToken);
                        _isConnected = true;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Failed to reconnect to SignalR hub: {Message}", ex.Message);
                        return; // Skip sending this update
                    }
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }
        
        if (_isConnected)
        {
            try
            {
                await _connection.InvokeAsync("SendMachineInfo", machineInfo, cancellationToken: cancellationToken);
                logger.LogDebug("Sent machine info for: {MachineName}", machineInfo.MachineName);
            }
            catch (Exception ex)
            {
                logger.LogError("Error during SignalR communication: {Message}", ex.Message);
                _isConnected = false;
                // Don't throw - we'll try again on the next update
            }
        }
        else
        {
            logger.LogWarning("Skipping machine info update - SignalR connection not available");
        }
    }
    
    public void Dispose()
    {
        _connectionLock?.Dispose();
        _connection?.DisposeAsync().AsTask().Wait();
    }
}