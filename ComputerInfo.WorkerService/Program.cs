using System.Runtime.InteropServices;
using ComputerInfo.WorkerService.Configuration;
using ComputerInfo.WorkerService.Services;
using ComputerInfo.WorkerService.Services.Adapters;
using ComputerInfo.WorkerService.Services.Ports;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.Configure<SignalRSettings>(builder.Configuration.GetSection("SignalRSettings"));
services.AddSingleton<ISignalRService, SignalRService>();
services.AddSingleton<IMachineInfoProvider>(_ =>
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        return new WindowsMachineInfoProvider();
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        return new LinuxMachineInfoProvider();
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        return new MacOsMachineInfoProvider();
    }

    throw new PlatformNotSupportedException();
});
services.AddHostedService<MachineInfoWorker>();

var host = builder.Build();
host.Run();