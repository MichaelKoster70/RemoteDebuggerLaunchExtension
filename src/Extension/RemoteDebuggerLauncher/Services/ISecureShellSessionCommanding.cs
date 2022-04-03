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
   internal interface ISecureShellSessionCommanding : IDisposable
   {
      /// <summary>
      /// Executes a SSH command asynchronous.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="Task{String}"/> holding the command response.</returns>
      /// <exception cref="SecureShellSessionException">thrown when the command failed.</exception>
      Task<string> ExecuteCommandAsync(string commandText);
   }
}
