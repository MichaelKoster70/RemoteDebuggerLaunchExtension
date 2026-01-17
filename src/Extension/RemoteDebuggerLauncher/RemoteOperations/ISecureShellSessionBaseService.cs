// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Interface defining the basic Secure Shell (SSH) session service.
   /// </summary>
   internal interface ISecureShellSessionBaseService
   {
      /// <summary>
      /// Gets the session settings.
      /// </summary>
      /// <value>The settings.</value>
      SecureShellSessionSettings Settings { get; }

      /// <summary>
      /// Executes a single SSH command asynchronous.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="Task{String}"/> holding the command response.</returns>
      /// <exception cref="SecureShellSessionException">thrown when the command failed.</exception>
      Task<string> ExecuteSingleCommandAsync(string commandText);

      /// <summary>
      /// Tries to executes a single SSH command asynchronous.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="Task{(int StatusCode, string Result, string Error)}"/> holding the command exit code and response and error text.</returns>
      Task<(int StatusCode, string Result, string Error)> TryExecuteCommandAsync(string commandText);

      /// <summary>
      /// Create a new SSH commanding session.
      /// </summary>
      /// <returns>The <see cref="ISecureShellSessionCommandingService"/> session instance.</returns>
      ISecureShellSessionCommandingService CreateCommandSession();
   }
}
