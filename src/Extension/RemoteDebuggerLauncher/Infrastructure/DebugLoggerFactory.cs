// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher.Infrastructure
{
   /// <summary>
   /// Factory for creating loggers. Exposed as a MEF component.
   /// </summary>
   [Export(typeof(ILoggerFactory))]
   internal sealed class DebugLoggerFactory : ILoggerFactory
   {
      private readonly SVsServiceProvider serviceProvider;
      private readonly string rootPathOverride;
      private ILoggerFactory loggerFactory;
      private bool initialized = false;
      private readonly object lockObject = new object();

      /// <summary>
      /// Initializes a new instance of the <see cref="LoggerFactory"/> class.
      /// </summary>
      /// <param name="serviceProvider">The service provider to get options.</param>
      [ImportingConstructor]
      public DebugLoggerFactory(SVsServiceProvider serviceProvider)
      {
         this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
         rootPathOverride = null;
      }

      public DebugLoggerFactory(string rootPathOverride)
      {
         this.rootPathOverride = rootPathOverride;
      }

      /// <inheritdoc />
      public ILogger CreateLogger(string categoryName)
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

      [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "To prevent unexpected failures")]
      private void EnsureInitialized()
      {
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
                  // Step 1: Prepare the logging configuration
                  // Create log file path
                  var logFilePath = CreateLogFilePath();

                  // Create the filter options
                  var filterOptions = new LoggerFilterOptions
                  {
                     MinLevel = minLogLevel
                  };

                  // Create the level overrides dictionary
                  var levelOverrides = new Dictionary<string, LogLevel>()
                  {
                     {  "Microsoft", LogLevel.Warning }
                  };

                  // Limit the logfile size to 1MB and keep up to 31 files
                  const long FileSizeLimitBytes = 1048576L;
                  const int FileCountLimit = 31;

                  // Step 2: Create the logger factory, add providers and configure them
                  loggerFactory = new LoggerFactory(Enumerable.Empty<ILoggerProvider>(), filterOptions);

                  // Add Debug logger provider for Output/Debug window, does not support filtering
                  loggerFactory.AddProvider(new DebugLoggerProvider());

                  // Add the Serilog file logger provider
                  _ = loggerFactory.AddFile(
                     logFilePath,
                     minimumLevel: minLogLevel,
                     levelOverrides: levelOverrides,
                     fileSizeLimitBytes: FileSizeLimitBytes,
                     retainedFileCountLimit: FileCountLimit,
                     outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {SourceContext}::{Message}{NewLine}{Exception}");
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

      private string CreateLogFilePath()
      {
         // Prepare log file path
         var localAppData = rootPathOverride ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
         var logDirectory = Path.Combine(localAppData, PackageConstants.FileSystem.StorageFolder, "Logfiles");
         var logFilePath = Path.Combine(logDirectory, "Debug-{Date}.log");

         // Ensure directory exists
         _ = Directory.CreateDirectory(logDirectory);

         return logFilePath;
      }
   }
}
