// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Text;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher.Logging
{
   /// <summary>
   /// A logger implementation that writes log messages to a file.
   /// </summary>
   internal class FileLogger : ILogger
   {
      private readonly string categoryName;
      private readonly string logFilePath;
      private readonly LogLevel minLogLevel;
      private readonly object lockObject = new object();

      /// <summary>
      /// Initializes a new instance of the <see cref="FileLogger"/> class.
      /// </summary>
      /// <param name="categoryName">The category name for the logger.</param>
      /// <param name="logFilePath">The path to the log file.</param>
      /// <param name="minLogLevel">The minimum log level to write.</param>
      public FileLogger(string categoryName, string logFilePath, LogLevel minLogLevel)
      {
         this.categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
         this.logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
         this.minLogLevel = minLogLevel;
      }

      /// <inheritdoc />
      public bool IsEnabled(LogLevel logLevel)
      {
         return logLevel != LogLevel.None && logLevel >= minLogLevel;
      }

      /// <inheritdoc />
      public void Log(LogLevel logLevel, string message)
      {
         if (!IsEnabled(logLevel))
         {
            return;
         }

         WriteLogEntry(logLevel, message, null);
      }

      /// <inheritdoc />
      public void Log(LogLevel logLevel, Exception exception, string message)
      {
         if (!IsEnabled(logLevel))
         {
            return;
         }

         WriteLogEntry(logLevel, message, exception);
      }

      private void WriteLogEntry(LogLevel logLevel, string message, Exception exception)
      {
         var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
         var logLevelString = GetLogLevelString(logLevel);
         
         var logEntry = new StringBuilder();
         logEntry.AppendFormat(CultureInfo.InvariantCulture, "[{0}] [{1}] {2}: {3}", timestamp, logLevelString, categoryName, message);
         
         if (exception != null)
         {
            logEntry.AppendLine();
            logEntry.AppendFormat(CultureInfo.InvariantCulture, "Exception: {0}", exception);
         }
         
         logEntry.AppendLine();

         lock (lockObject)
         {
            try
            {
               // Ensure directory exists
               var directory = Path.GetDirectoryName(logFilePath);
               if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
               {
                  Directory.CreateDirectory(directory);
               }

               File.AppendAllText(logFilePath, logEntry.ToString());
            }
            catch
            {
               // Silently fail if we can't write to the log file
               // We don't want logging failures to crash the extension
            }
         }
      }

      private static string GetLogLevelString(LogLevel logLevel)
      {
         return logLevel switch
         {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO ",
            LogLevel.Warning => "WARN ",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRIT ",
            _ => "UNKN "
         };
      }
   }
}
