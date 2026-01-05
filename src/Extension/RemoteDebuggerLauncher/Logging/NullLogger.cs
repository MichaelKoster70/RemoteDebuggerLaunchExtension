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
   /// A logger implementation that does nothing. Used when logging is disabled.
   /// </summary>
   internal class NullLogger : ILogger
   {
      /// <inheritdoc />
      public bool IsEnabled(LogLevel logLevel) => false;

      /// <inheritdoc />
      public void Log(LogLevel logLevel, string message)
      {
         // Do nothing
      }

      /// <inheritdoc />
      public void Log(LogLevel logLevel, Exception exception, string message)
      {
         // Do nothing
      }
   }
}
