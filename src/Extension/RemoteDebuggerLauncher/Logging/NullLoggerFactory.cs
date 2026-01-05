// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace RemoteDebuggerLauncher.Logging
{
   /// <summary>
   /// A logger factory that creates null loggers. Used when logging is disabled.
   /// </summary>
   internal sealed class NullLoggerFactory : ILoggerFactory
   {
      /// <inheritdoc />
      public ILogger CreateLogger(string categoryName) => NullLogger.Instance;

      /// <inheritdoc />
      public void AddProvider(ILoggerProvider provider)
      {
         // EMPTY_BODY
      }

      /// <inheritdoc />
      public void Dispose()
      {
         // EMPTY_BODY
      }
   }
}
