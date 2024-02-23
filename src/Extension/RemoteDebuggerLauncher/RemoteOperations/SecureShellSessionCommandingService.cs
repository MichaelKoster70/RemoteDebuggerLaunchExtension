// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Secure Shell (SSH) commanding session executing multiple commands in a single SSH session.
   /// Implements the <see cref="ISecureShellSessionCommandingService"/> interface.
   /// </summary>
   /// <seealso cref="ISecureShellSessionCommandingService"/>
   internal class SecureShellSessionCommandingService : ISecureShellSessionCommandingService
   {
      private readonly SshClient client;
      private bool disposedValue;

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellSessionCommandingService"/> class.
      /// </summary>
      /// <param name="client">The SSH client.</param>
      public SecureShellSessionCommandingService(SshClient client)
      {
         this.client = client;
      }

      /// <inheritdoc/>
      public Task<string> ExecuteCommandAsync(string commandText)
      {
         ThrowIf.ArgumentNullOrEmpty(commandText, nameof(commandText));

         return Task.Run(() =>
         {
            try
            {
               EnsureConnected();

               using (var command = client.RunCommand(commandText))
               {
                  if (command.ExitStatus != 0)
                  {
                     throw new SecureShellSessionException(command.Error, command.ExitStatus);
                  }

                  return command.Result;
               }
            }
            catch (SshException e)
            {
               throw new SecureShellSessionException(e.Message, e);
            }
            catch (InvalidOperationException e)
            {
               throw new SecureShellSessionException(e.Message, e);
            }
         });
      }

      /// <inheritdoc/>
      public Task<(int StatusCode, string Result, string Error)> TryExecuteCommandAsync(string commandText)
      {
         ThrowIf.ArgumentNullOrEmpty(commandText, nameof(commandText));

         return Task.Run(() =>
         {
            try
            {
               EnsureConnected();

               using (var command = client.RunCommand(commandText))
               {
                  return (command.ExitStatus, command.Result, command.Error);
               }
            }
            catch (SshException e)
            {
               throw new SecureShellSessionException(e.Message, e);
            }
            catch (InvalidOperationException e)
            {
               throw new SecureShellSessionException(e.Message, e);
            }
         });
      }

      private void EnsureConnected()
      {
         if (!client.IsConnected)
         {
            client.Connect();
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
               client?.Dispose();
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
