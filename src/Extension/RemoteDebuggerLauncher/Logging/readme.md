# Logging Infrastructure

This directory contains the logging infrastructure for the Remote Debugger Launcher extension.

## Overview

The logging system uses **Serilog** with Microsoft's `Microsoft.Extensions.Logging` integration and provides:

- **LogLevel-based filtering**: Trace, Debug, Information, Warning, Error, Critical, and None
- **File-based logging**: Logs are written to `%localappdata%\RemoteDebuggerLauncher\Logfiles`
- **Structured logging**: Serilog provides rich structured logging capabilities
- **MEF integration**: Loggers can be injected via MEF's `[ImportingConstructor]`
- **Configuration**: Logging level can be configured in Visual Studio Options

## Packages Used

- **Serilog**: Core structured logging library
- **Serilog.Extensions.Logging**: Bridge between Serilog and Microsoft.Extensions.Logging
- **Serilog.Sinks.File**: File sink for writing logs to disk
- **Microsoft.Extensions.Logging.Abstractions**: Standard logging abstractions

## Files

- **FileLoggerFactory.cs**: MEF-exportable factory that configures Serilog and creates loggers
- **NullLogger.cs**: No-op logger used when logging is disabled
- **NullLoggerFactory.cs**: Factory that creates null loggers when logging is disabled

## Usage

### Injecting a Logger via MEF

```csharp
using Microsoft.Extensions.Logging;

[Export]
internal class MyService
{
    private readonly ILogger logger;

    [ImportingConstructor]
    public MyService(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(nameof(MyService));
    }

    public void DoWork()
    {
        logger.LogInformation("Starting work");
        try
        {
            // Do work
            logger.LogDebug("Work completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Work failed");
        }
    }
}
```

### Configuration

Users can configure the logging level in Visual Studio:
1. Go to **Tools > Options**
2. Navigate to **RemoteDebuggerLauncher > Local**
3. Set the **Log level** property to the desired level
4. Set to **None** to disable logging

## Log File Location

Log files are created in: `%localappdata%\RemoteDebuggerLauncher\Logfiles\RemoteDebuggerLauncher-{timestamp}.log`

Each session creates a new log file with a timestamp in the format: `yyyyMMdd-HHmmss`

## Log Format

Serilog uses the following format template:

```
[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u5}] {SourceContext}: {Message:lj}{NewLine}{Exception}
```

Example output:

```
[2024-01-05 07:30:45.123] [INFO ] SecureShellKeySetupService: Starting server fingerprint registration for user@host:22
[2024-01-05 07:30:45.456] [ERROR] SecureShellKeySetupService: Failed to register server fingerprint
System.InvalidOperationException: Connection failed
   at ...
```
