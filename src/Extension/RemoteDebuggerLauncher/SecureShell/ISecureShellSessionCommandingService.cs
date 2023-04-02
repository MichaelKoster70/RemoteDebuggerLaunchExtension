// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface for the Secure Shell (SSH) commanding session executing multiple commands in a single SSH session.
   /// </summary>
   internal interface ISecureShellSessionCommandingService : IDisposable
   {
      /// <summary>
      /// Executes a SSH command asynchronous.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="Task{String}"/> holding the command response.</returns>
      /// <exception cref="SecureShellSessionException">thrown when the command failed.</exception>
      Task<string> ExecuteCommandAsync(string commandText);

      /// <summary>
      /// Tries to executes a SSH command asynchronous.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="Task{(int StatusCode, string Response)}"/> holding the command exit code and response.</returns>
      Task<(int StatusCode, string Response)> TryExecuteCommandAsync(string commandText);
   }
}
