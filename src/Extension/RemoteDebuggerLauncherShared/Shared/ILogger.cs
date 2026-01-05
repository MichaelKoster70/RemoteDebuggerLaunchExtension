// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;

namespace RemoteDebuggerLauncher.Shared
{
   /// <summary>
   /// Represents a type used to perform logging.
   /// Inspired by the .NET ILogger abstraction.
   /// </summary>
   public interface ILogger
   {
      /// <summary>
      /// Writes a log entry.
      /// </summary>
      /// <param name="logLevel">Entry will be written on this level.</param>
      /// <param name="message">Format string of the log message.</param>
      void Log(LogLevel logLevel, string message);

      /// <summary>
      /// Writes a log entry.
      /// </summary>
      /// <param name="logLevel">Entry will be written on this level.</param>
      /// <param name="exception">The exception to log.</param>
      /// <param name="message">Format string of the log message.</param>
      void Log(LogLevel logLevel, Exception exception, string message);

      /// <summary>
      /// Checks if the given logLevel is enabled.
      /// </summary>
      /// <param name="logLevel">Level to be checked.</param>
      /// <returns><c>true</c> if enabled.</returns>
      bool IsEnabled(LogLevel logLevel);
   }
}
