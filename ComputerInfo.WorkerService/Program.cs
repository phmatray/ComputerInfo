using ComputerInfo.WorkerService;
using ComputerInfo.WorkerService.Configuration;
using ComputerInfo.WorkerService.Services;
using ComputerInfo.WorkerService.Services.Adapters;
using ComputerInfo.WorkerService.Services.Ports;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.Configure<SignalRSettings>(builder.Configuration.GetSection("SignalRSettings"));
services.AddSingleton<ISignalRService, SignalRService>();
services.AddSingleton<IMachineInfoProvider, MachineInfoProvider>();
services.AddHostedService<MachineInfoWorker>();

var host = builder.Build();
host.Run();