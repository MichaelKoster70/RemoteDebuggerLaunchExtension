// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

namespace RemoteDebuggerLauncher.Infrastructure
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
         return NullScope.Instance;
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

      /// <summary>
      /// A no-op disposable for scope handling.
      /// </summary>
      private sealed class NullScope : IDisposable
      {
         public static NullScope Instance { get; } = new NullScope();

         private NullScope()
         {
         }

         public void Dispose()
         {
            // Do nothing
         }
      }
   }
}
