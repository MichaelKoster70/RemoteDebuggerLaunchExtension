// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class providing <see cref="ISecureShellSessionService"/> factorties.
   /// </summary>
   internal static class SecureShellSession
   {
      /// <summary>
      /// Creates an <see cref="ISecureShellSessionService"/> instance with settings read from the supplied configuration.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <returns>An <see cref="SecureShellSession"/> instance</returns>
      /// <exception cref="ArgumentNullException">configurationAggregator is null.</exception>
      public static ISecureShellSessionService Create(ConfigurationAggregator configurationAggregator)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));
         var settings = SecureShellSessionSettings.Create(configurationAggregator);

         return new SecureShellSessionService(settings);
      }
   }
}
