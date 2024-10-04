# Computer Information Collector - POC

## Overview

This project is a **Proof of Concept (POC)** for a cross-platform service that collects system and machine information such as memory, disk usage, CPU details, and network adapters. The service is designed to run on **Windows**, **Linux**, and **macOS** platforms, providing detailed insights into the system's current state.

The project demonstrates a clean, maintainable, and extendable architecture by splitting platform-specific logic into separate services. The implementation relies on **dependency injection** to choose the appropriate provider based on the host operating system.

## Features

- **Cross-platform compatibility**: Supports **Windows**, **Linux**, and **macOS**.
- **Platform-specific implementations**: Uses different strategies for each platform to retrieve machine information, such as memory and disk usage.
- **Modular architecture**: The platform-specific logic is neatly separated into individual services, making the code more maintainable and easy to extend.
- **System information collected**:
    - Machine name
    - Operating system description and architecture
    - CPU architecture and processor count
    - Total and available memory
    - Disk drives information (total size, free space)
    - Network adapter details (IP address, MAC address)
    - System uptime

## Components

### 1. `IMachineInfoProvider`
An interface that defines the contract for collecting machine information.

### 2. Platform-specific Providers
Three separate implementations of `IMachineInfoProvider`, each tailored to a specific platform:
- **`WindowsMachineInfoProvider`**: Collects machine info using Windows APIs like `PerformanceCounter`.
- **`MacOSMachineInfoProvider`**: Uses `sysctl` and `vm_stat` commands to gather system info on macOS.
- **`UnixMachineInfoProvider`**: Retrieves information from `/proc/meminfo` and other system files for Linux-based systems.

### 3. Dependency Injection
The project uses **dependency injection** to select the appropriate `IMachineInfoProvider` implementation based on the current platform. This ensures that platform-specific logic is isolated and only the relevant provider is used.

### 4. `MachineInfoWorker`
A background service that periodically collects and sends system information. It leverages the `IMachineInfoProvider` to gather system details and can be easily adapted to send or log the information to various outputs (e.g., APIs, log files).

## Setup & Usage

### Prerequisites

- .NET 8.0 or later
- The project is compatible with **Windows**, **Linux**, and **macOS**.

### Clone the Repository

```bash
git clone https://github.com/phmatray/ComputerInfo.git
cd ComputerInfo
```

### Build and Run the Project

```bash
dotnet build
dotnet run
```

### Dependency Injection Setup

The platform-specific provider is injected using the following logic in `Program.cs`:

```csharp
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    services.AddSingleton<IMachineInfoProvider, WindowsMachineInfoProvider>();
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    services.AddSingleton<IMachineInfoProvider, MacOSMachineInfoProvider>();
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    services.AddSingleton<IMachineInfoProvider, UnixMachineInfoProvider>();
}
```

### Customization

You can modify or extend the project by adding additional platform-specific providers or collecting more machine details. The architecture is designed to be modular and adaptable.

## Architecture

The architecture follows the **Single Responsibility Principle (SRP)**, with platform-specific logic abstracted into separate classes. This modular approach ensures maintainability and scalability, allowing future improvements or the addition of more platforms without complicating the existing structure.

### Flow

1. **Startup**: During startup, the appropriate `IMachineInfoProvider` is injected based on the current operating system.
2. **Execution**: The `MachineInfoWorker` runs as a background service, periodically invoking the `IMachineInfoProvider` to retrieve machine information.
3. **Logging**: For demonstration purposes, the collected information is logged to the console. In a production scenario, this could be extended to send the information to an API or save it to a database.

## Limitations

- This is a **POC** and may not handle all edge cases or offer full error handling for production use.
- Platform-specific implementations might need further testing and adjustments depending on the target system configuration.
- The `PerformanceCounter` API used for memory info on Windows may require elevated permissions on some systems.

## Contributing

Feel free to fork the repository and submit pull requests. Contributions to improve platform support or add new features are welcome!

## License

This project is licensed under the MIT License.
