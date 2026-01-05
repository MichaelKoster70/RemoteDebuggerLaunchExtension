// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

namespace RemoteDebuggerLauncher.Logging
{
   /// <summary>
   /// A logger implementation that does nothing. Used when logging is disabled.
   /// </summary>
   internal class NullLogger : ILogger
   {
      /// <summary>
      /// Gets the singleton instance of the null logger.
      /// </summary>
      public static NullLogger Instance { get; } = new NullLogger();

      /// <inheritdoc />
      public IDisposable BeginScope<TState>(TState state)
      {
         return null;
      }

      /// <inheritdoc />
      public bool IsEnabled(LogLevel logLevel)
      {
         return false;
      }

      /// <inheritdoc />
      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
      {
         // Do nothing
      }
   }
}
