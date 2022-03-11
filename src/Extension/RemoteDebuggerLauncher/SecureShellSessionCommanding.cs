// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Renci.SshNet;

namespace RemoteDebuggerLauncher
{
   internal class SecureShellSessionCommanding : ISecureShellSessionCommanding
   {
      private readonly SshClient client;
      private bool disposedValue;

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellSessionCommanding"/> class.
      /// </summary>
      /// <param name="client">The SSH client.</param>
      public SecureShellSessionCommanding(SshClient client)
      {
         this.client = client;
      }

      /// <summary>
      /// Executes a SSH command asynchronous.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="Task{String}" /> holding the command response.</returns>
      public Task<string> ExecuteCommandAsync(string commandText)
      {
         return Task.Run(() => ExecuteCommand(commandText));
      }

      /// <summary>
      /// Executes a SSH command.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="String" /> holding the command response.</returns>
      public string ExecuteCommand(string commandText)
      {
         EnsureConnected();
         using (var command = client.RunCommand(commandText))
         {
            return command.Result;
         }
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
