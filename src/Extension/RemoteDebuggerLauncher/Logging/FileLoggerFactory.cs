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
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher.Logging
{
   /// <summary>
   /// Factory for creating file-based loggers. Exposed as a MEF component.
   /// </summary>
   [Export(typeof(ILoggerFactory))]
   internal class FileLoggerFactory : ILoggerFactory
   {
      private readonly string logFilePath;
      private readonly LogLevel minLogLevel;
      private readonly IOptionsPageAccessor optionsPageAccessor;

      /// <summary>
      /// Initializes a new instance of the <see cref="FileLoggerFactory"/> class.
      /// </summary>
      /// <param name="optionsPageAccessor">The options page accessor to get logging configuration.</param>
      [ImportingConstructor]
      public FileLoggerFactory([Import(typeof(SOptionsPageAccessor))] IOptionsPageAccessor optionsPageAccessor)
      {
         this.optionsPageAccessor = optionsPageAccessor ?? throw new ArgumentNullException(nameof(optionsPageAccessor));
         
         // Get logging level from options
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

      /// <inheritdoc />
      public ILogger CreateLogger(string categoryName)
      {
         if (minLogLevel == LogLevel.None)
         {
            return new NullLogger();
         }

         return new FileLogger(categoryName, logFilePath, minLogLevel);
      }
   }
}
