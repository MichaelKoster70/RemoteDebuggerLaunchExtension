// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.Shared
{
   /// <summary>
   /// Represents a type used to configure the logging system and create instances of <see cref="ILogger"/>.
   /// </summary>
   public interface ILoggerFactory
   {
      /// <summary>
      /// Creates a new <see cref="ILogger"/> instance.
      /// </summary>
      /// <param name="categoryName">The category name for messages produced by the logger.</param>
      /// <returns>The <see cref="ILogger"/> instance.</returns>
      ILogger CreateLogger(string categoryName);
   }
}
