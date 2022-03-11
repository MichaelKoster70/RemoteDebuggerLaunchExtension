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
using Renci.SshNet;

namespace RemoteDebuggerLauncher
{
   internal class SecureShellSession : IDisposable
   {
      private bool disposedValue;

      private SshClient sshClient; // == null
      private readonly SecureShellSessionSettings settings;

      internal SecureShellSession(SecureShellSessionSettings settings)
      {
         this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
      }

      /// <summary>
      /// Gets the session settings.
      /// </summary>
      /// <value>The settings.</value>
      public SecureShellSessionSettings Settings => settings;

      /// <summary>
      /// Creates an <see cref="SecureShellSession"/> with settings read from the supplied configuration.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <returns>An <see cref="SecureShellSession"/> instance</returns>
      /// <exception cref="ArgumentNullException">configurationAggregator is null.</exception>
      public static SecureShellSession Create(ConfigurationAggregator configurationAggregator)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));
         var settings = SecureShellSessionSettings.Create(configurationAggregator);

         return new SecureShellSession(settings);
      }

      /// <summary>
      /// Executes a single SSH command asynchronous.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="Task{String}"/> holding the command response.</returns>
      public Task<string> ExecuteSingleCommandAsync(string commandText)
      {
         return Task.Run(() => ExecuteSingleCommand(commandText));
      }

      /// <summary>
      /// Executes single SSH command.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="String"/> holding the command response.</returns>
      public string ExecuteSingleCommand(string commandText)
      {
         using (var client = CreateSshClient())
         {
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
            client.Connect();
            using (var command = client.RunCommand(commandText))
            {
               return command.Result;
            }            
         }
      }

      public ISecureShellSessionCommanding CreateCommandSession()
      {
         var client = CreateSshClient();
         client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
         return new SecureShellSessionCommanding(client);
      }

      private SshClient CreateSshClient()
      {
         switch (settings.Authentication)
         {
            case AuthenticationKind.Password:
               return new SshClient(settings.HostName, settings.UserName, "");
            case AuthenticationKind.PrivateKey:
               var key = new PrivateKeyFile(settings.PrivateKeyFile);
               return new SshClient(settings.HostName, settings.UserName, key);
            default:
               throw new InvalidOperationException("unsupported authentication");
         }
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
               sshClient?.Dispose();
               sshClient = null;
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            disposedValue = true;
         }
      }

      /// <summary>
      /// Disposes this instance.
      /// </summary>
      public void Dispose()
      {
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
   }
}
