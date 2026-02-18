# Running the ComputerInfo Application

This application consists of two services that need to be running:

1. **Blazor App** - The web dashboard (must be started first)
2. **Worker Service** - The system monitor that sends data

## Quick Start

### Option 1: Using Two Terminal Windows

**Terminal 1 - Start the Blazor App first:**
```bash
cd ComputerInfo.BlazorApp
dotnet run
```

Wait until you see:
```
Now listening on: http://localhost:5042
```

**Terminal 2 - Start the Worker Service:**
```bash
cd ComputerInfo.WorkerService
dotnet run
```

### Option 2: Using a Single Terminal with Background Process

```bash
# Start Blazor app in the background
cd ComputerInfo.BlazorApp
dotnet run &

# Wait a few seconds for it to start
sleep 5

# Start Worker Service
cd ../ComputerInfo.WorkerService
dotnet run
```

### Option 3: Using IDE (Visual Studio / Rider)

1. Configure multiple startup projects
2. Set both projects to start
3. Ensure Blazor App starts before Worker Service

## Accessing the Dashboard

Once both services are running, open your browser to:
http://localhost:5042

## Troubleshooting

### "Connection refused" error

If you see this error, it means the Worker Service couldn't connect to the Blazor app. 

**Solution:**
1. Make sure the Blazor app is running first
2. Verify it's listening on port 5042
3. The Worker Service will automatically retry the connection

### No data showing in the dashboard

1. Check that both services are running
2. Look for "SignalR client connected successfully" in the Worker Service logs
3. Refresh the browser page

## Architecture Notes

- The Worker Service will retry connecting to the Blazor app with exponential backoff
- If the Blazor app restarts, the Worker Service will automatically reconnect
- Multiple Worker Services can connect to the same Blazor app (for monitoring multiple machines)