using System.Net;

namespace ComputerInfo.WorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5001/");
        listener.Start();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            string machineName = Environment.MachineName;

            // Write response to the HTTP request
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(machineName);
            context.Response.ContentLength64 = buffer.Length;
            Stream output = context.Response.OutputStream;
            await output.WriteAsync(buffer, stoppingToken);
            output.Close();
            
            // if (_logger.IsEnabled(LogLevel.Information))
            // {
            //     _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //     
            //     string machineName = Environment.MachineName;
            //     _logger.LogInformation("Machine name: {MachineName}", machineName);
            // }
            //
            // await Task.Delay(60000, stoppingToken);
        }
    }
}