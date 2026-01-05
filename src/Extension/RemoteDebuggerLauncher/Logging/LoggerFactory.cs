// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shell;
using Serilog;
using Serilog.Extensions.Logging;

namespace RemoteDebuggerLauncher.Logging
{
   /// <summary>
   /// Factory for creating loggers. Exposed as a MEF component.
   /// </summary>
   [Export(typeof(ILoggerFactory))]
   internal class LoggerFactory : ILoggerFactory
   {
      private readonly SVsServiceProvider serviceProvider;
      private Microsoft.Extensions.Logging.ILoggerFactory loggerFactory;
      private bool initialized = false;
      private readonly object lockObject = new object();

      /// <summary>
      /// Initializes a new instance of the <see cref="LoggerFactory"/> class.
      /// </summary>
      /// <param name="serviceProvider">The service provider to get options.</param>
      [ImportingConstructor]
      public LoggerFactory(SVsServiceProvider serviceProvider)
      {
         this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
      }

      /// <inheritdoc />
      public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
      {
         EnsureInitialized();
         return loggerFactory.CreateLogger(categoryName);
      }

      /// <inheritdoc />
      public void AddProvider(ILoggerProvider provider)
      {
         EnsureInitialized();
         loggerFactory.AddProvider(provider);
      }

      /// <inheritdoc />
      public void Dispose()
      {
         loggerFactory?.Dispose();
      }

      private void EnsureInitialized()
      {
         if (initialized)
         {
            return;
         }

         lock (lockObject)
         {
            if (initialized)
            {
               return;
            }

            try
            {
               // Get the options page accessor service
               LogLevel minLogLevel = LogLevel.None;

               if (serviceProvider.GetService(typeof(SOptionsPageAccessor)) is IOptionsPageAccessor optionsPageAccessor)
               {
                  minLogLevel = optionsPageAccessor.QueryLogLevel();
               }

               if (minLogLevel == LogLevel.None)
               {
                  // Create a null logger factory when logging is disabled
                  loggerFactory = new NullLoggerFactory();
               }
               else
               {
                  // Configure Serilog
                  var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                  var logDirectory = Path.Combine(localAppData, "RemoteDebuggerLauncher", "Logfiles");
                  var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                  var logFilePath = Path.Combine(logDirectory, $"RemoteDebuggerLauncher-{timestamp}.log");

                  // Ensure directory exists
                  if (!Directory.Exists(logDirectory))
                  {
                     _ = Directory.CreateDirectory(logDirectory);
                  }

                  // Configure Serilog logger
                  var serilogLogger = new LoggerConfiguration()
                     .MinimumLevel.Is(MapToSerilogLevel(minLogLevel))
                     .WriteTo.File(
                        logFilePath,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u5}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                        formatProvider: CultureInfo.InvariantCulture)
                     .CreateLogger();

                  // Create Microsoft.Extensions.Logging factory with Serilog
                  loggerFactory = new SerilogLoggerFactory(serilogLogger, dispose: true);
               }
            }
            catch
            {
               // If we can't configure logging, use null logger factory
               loggerFactory = new NullLoggerFactory();
            }

            initialized = true;
         }
      }

      private static Serilog.Events.LogEventLevel MapToSerilogLevel(LogLevel logLevel)
      {
         switch (logLevel)
         {
            case LogLevel.Trace:
               return Serilog.Events.LogEventLevel.Verbose;
            case LogLevel.Debug:
               return Serilog.Events.LogEventLevel.Debug;
            case LogLevel.Information:
               return Serilog.Events.LogEventLevel.Information;
            case LogLevel.Warning:
               return Serilog.Events.LogEventLevel.Warning;
            case LogLevel.Error:
               return Serilog.Events.LogEventLevel.Error;
            case LogLevel.Critical:
               return Serilog.Events.LogEventLevel.Fatal;
            default:
               return Serilog.Events.LogEventLevel.Information;
         }
      }
   }
}
