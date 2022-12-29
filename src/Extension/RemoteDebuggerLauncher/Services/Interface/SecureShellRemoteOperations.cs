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
   internal static class SecureShellRemoteOperations
   {
      /// <summary>
      /// Creates a <see cref="ISecureShellRemoteOperationsService" /> with settings read from the supplied configuration.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator to read the settings from.</param>
      /// <param name="logger">The logger instance to use.</param>
      /// <param name="statusbar">Optional statusbar service to report progress.</param>
      /// <returns>A <see cref="SecureShellRemoteOperations" /> instance.</returns>
      /// <exception cref="ArgumentNullException">configurationAggregator is null.</exception>
      /// <exception cref="ArgumentNullException">logger is null.</exception>
      public static ISecureShellRemoteOperationsService Create(ConfigurationAggregator configurationAggregator, ILoggerService logger, IStatusbarService statusbar = null)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));
         ThrowIf.ArgumentNull(logger, nameof(logger));

         var session = SecureShellSession.Create(configurationAggregator);
         return new SecureShellRemoteOperationsService(configurationAggregator, session, logger, statusbar);
      }
   }
}
