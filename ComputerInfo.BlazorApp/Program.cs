using ComputerInfo.BlazorApp;
using ComputerInfo.BlazorApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR to the service collection
builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Map the SignalR hub endpoint
app.MapHub<MachineHub>("/machinehub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();