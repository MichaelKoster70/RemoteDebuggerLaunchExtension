# Logging Infrastructure

This directory contains the logging infrastructure for the Remote Debugger Launcher extension.

## Overview

The logging system uses Microsoft's `Microsoft.Extensions.Logging` package and provides:

- **LogLevel-based filtering**: Trace, Debug, Information, Warning, Error, Critical, and None
- **File-based logging**: Logs are written to `%localappdata%\RemoteDebuggerLauncher\Logfiles`
- **MEF integration**: Loggers can be injected via MEF's `[ImportingConstructor]`
- **Configuration**: Logging level can be configured in Visual Studio Options

## Files

- **FileLogger.cs**: Implementation of file-based logger using `Microsoft.Extensions.Logging.ILogger`
- **FileLoggerFactory.cs**: MEF-exportable factory implementing `Microsoft.Extensions.Logging.ILoggerFactory`
- **NullLogger.cs**: No-op logger used when logging is disabled

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

```
[2024-01-05 07:30:45.123] [INFO ] CategoryName: Message text
[2024-01-05 07:30:45.456] [ERROR] CategoryName: Error message
Exception: System.InvalidOperationException: Error details
   at ...
```
