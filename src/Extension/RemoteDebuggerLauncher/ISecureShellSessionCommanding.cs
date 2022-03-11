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
      Task<string> ExecuteCommandAsync(string commandText);

      /// <summary>
      /// Executes a SSH command.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="String"/> holding the command response.</returns>
      string ExecuteCommand(string commandText);
   }
}
