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
using System.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.Extensions.Logging.Debug;

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
                  // Create a configured Microsoft.Extensions.Logging LoggerFactory with filter options
                  var filterOptions = new LoggerFilterOptions
                  {
                     MinLevel = minLogLevel
                  };
                  loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory(Enumerable.Empty<ILoggerProvider>(), filterOptions);

                  var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                  var logDirectory = Path.Combine(localAppData, "RemoteDebuggerLauncher", "Logfiles");
                  var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                  var logFilePath = Path.Combine(logDirectory, $"RemoteDebuggerLauncher-{timestamp}.log");

                  // Ensure directory exists
                  if (!Directory.Exists(logDirectory))
                  {
                     _ = Directory.CreateDirectory(logDirectory);
                  }

                  // Add Debug logger provider for Output/Debug window
                  loggerFactory.AddProvider(new DebugLoggerProvider());

                  // Use Serilog.Extensions.Logging.File to add a file logger
                  loggerFactory.AddFile(
                     logFilePath,
                     minimumLevel: minLogLevel,
                     outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u5}] {CategoryName}: {Message}{NewLine}{Exception}");
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
   }
}
