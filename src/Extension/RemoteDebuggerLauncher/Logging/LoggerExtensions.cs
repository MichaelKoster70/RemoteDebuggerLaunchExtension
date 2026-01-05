// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher.Logging
{
   /// <summary>
   /// Extension methods for <see cref="ILogger"/> to provide convenient logging methods.
   /// </summary>
   internal static class LoggerExtensions
   {
      /// <summary>
      /// Formats and writes a trace log message.
      /// </summary>
      public static void LogTrace(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Trace, message);
      }

      /// <summary>
      /// Formats and writes a debug log message.
      /// </summary>
      public static void LogDebug(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Debug, message);
      }

      /// <summary>
      /// Formats and writes an informational log message.
      /// </summary>
      public static void LogInformation(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Information, message);
      }

      /// <summary>
      /// Formats and writes a warning log message.
      /// </summary>
      public static void LogWarning(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Warning, message);
      }

      /// <summary>
      /// Formats and writes a warning log message.
      /// </summary>
      public static void LogWarning(this ILogger logger, Exception exception, string message)
      {
         logger.Log(LogLevel.Warning, exception, message);
      }

      /// <summary>
      /// Formats and writes an error log message.
      /// </summary>
      public static void LogError(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Error, message);
      }

      /// <summary>
      /// Formats and writes an error log message.
      /// </summary>
      public static void LogError(this ILogger logger, Exception exception, string message)
      {
         logger.Log(LogLevel.Error, exception, message);
      }

      /// <summary>
      /// Formats and writes a critical log message.
      /// </summary>
      public static void LogCritical(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Critical, message);
      }

      /// <summary>
      /// Formats and writes a critical log message.
      /// </summary>
      public static void LogCritical(this ILogger logger, Exception exception, string message)
      {
         logger.Log(LogLevel.Critical, exception, message);
      }
   }
}
