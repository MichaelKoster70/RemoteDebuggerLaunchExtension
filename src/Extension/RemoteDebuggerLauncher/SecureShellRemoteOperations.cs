// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Holds the high level operations performed on the remote device
   /// </summary>
   internal class SecureShellRemoteOperations : IDisposable
   {
      private readonly ConfigurationAggregator configurationAggregator;
      private readonly SecureShellSession session;
      private readonly ILoggerService logger;
      private bool disposedValue;

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellRemoteOperations" /> class.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <param name="session">The session to use.</param>
      /// <param name="logger">The logger service instance to use.</param>
      internal SecureShellRemoteOperations(ConfigurationAggregator configurationAggregator, SecureShellSession session, ILoggerService logger)
      {
         this.configurationAggregator = configurationAggregator;
         this.session = session;
         this.logger = logger;
      }

      /// <summary>
      /// Creates a <see cref="SecureShellRemoteOperations"/> with settings read from the supplied configuration.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <returns>SecureShellOperations.</returns>
      /// <exception cref="ArgumentNullException">configurationAggregator is null.</exception>
      public static SecureShellRemoteOperations Create(ConfigurationAggregator configurationAggregator, ILoggerService logger)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));

         var session = SecureShellSession.Create(configurationAggregator);
         return new SecureShellRemoteOperations(configurationAggregator, session, logger);
      }

      /// <summary>
      /// Checks whether a connection with the remove device can be established.
      /// </summary>
      /// <returns>A Task representing the asynchronous operation.</returns>
      /// <exception cref="RemoteDebuggerLauncherException">Thrown when connection cannot be established.</exception>
      public async Task CheckConnectionThrowAsync()
      {
         try
         {
            logger.WriteOutputDebugPane($"connecting to {session.Settings.UserName}@{ session.Settings.HostName}...");
            await session.ExecuteSingleCommandAsync("hello echo").ConfigureAwait(true);
            logger.WriteOutputDebugPane("OK\r\n");
         }
         catch (Exception ex)
         {
            // whatever exception is thrown indicates a problem
            var message = $"Cannot connect to {session.Settings.UserName}@{ session.Settings.HostName} : {ex.Message}";
            logger.WriteOutputDebugPane("FAILED\r\n");
            throw new RemoteDebuggerLauncherException(message);
         }
      }

      public async Task<bool> TryInstallVsDbgAsync()
      {
         logger.WriteOutputDebugPane($"connecting to {session.Settings.UserName}@{ session.Settings.HostName}...");
         return true;
      }

      /// <summary>
      /// Releases unmanaged and - optionally - managed resources.
      /// </summary>
      /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
      protected virtual void Dispose(bool disposing)
      {
         if (!disposedValue)
         {
            if (disposing)
            {
               // dispose managed state (managed objects)
               session?.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            disposedValue = true;
         }
      }

      public void Dispose()
      {
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
   }
}
