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

namespace RemoteDebuggerLauncher.Logging
{
   /// <summary>
   /// Factory for creating file-based loggers. Exposed as a MEF component.
   /// </summary>
   [Export(typeof(ILoggerFactory))]
   internal class FileLoggerFactory : ILoggerFactory
   {
      private readonly SVsServiceProvider serviceProvider;
      private string logFilePath;
      private LogLevel minLogLevel = LogLevel.None;
      private bool initialized = false;
      private readonly object lockObject = new object();

      /// <summary>
      /// Initializes a new instance of the <see cref="FileLoggerFactory"/> class.
      /// </summary>
      /// <param name="serviceProvider">The service provider to get options.</param>
      [ImportingConstructor]
      public FileLoggerFactory(SVsServiceProvider serviceProvider)
      {
         this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
      }

      /// <inheritdoc />
      public ILogger CreateLogger(string categoryName)
      {
         EnsureInitialized();

         if (minLogLevel == LogLevel.None)
         {
            return NullLogger.Instance;
         }

         return new FileLogger(categoryName, logFilePath, minLogLevel);
      }

      /// <inheritdoc />
      /// <remarks>This simple implementation does not support adding providers.</remarks>
      public void AddProvider(ILoggerProvider provider)
      {
         throw new NotSupportedException("This logger factory does not support adding providers.");
      }

      /// <inheritdoc />
      public void Dispose()
      {
         // Nothing to dispose
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
               var optionsPageAccessor = serviceProvider.GetService(typeof(SOptionsPageAccessor)) as IOptionsPageAccessor;
               if (optionsPageAccessor != null)
               {
                  minLogLevel = optionsPageAccessor.QueryLogLevel();

                  // Determine log file path
                  if (minLogLevel != LogLevel.None)
                  {
                     var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                     var logDirectory = Path.Combine(localAppData, "RemoteDebuggerLauncher", "Logfiles");
                     var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                     logFilePath = Path.Combine(logDirectory, $"RemoteDebuggerLauncher-{timestamp}.log");
                  }
               }
            }
            catch
            {
               // If we can't get the options, default to no logging
               minLogLevel = LogLevel.None;
            }

            initialized = true;
         }
      }
   }
}
