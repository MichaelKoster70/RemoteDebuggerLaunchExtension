// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining the deploy via SSH service.
   /// </summary>
   internal interface ISecureShellDeployService
   {
      /// <summary>
      /// Starts the deploy operation.
      /// </summary>
      /// <param name="checkConnection"><c>true</c> to check the connection to the target; else <c>false</c>.</param>
      /// <param name="logHost"><c>true</c> to log host information.</param>
      /// <returns>The <see cref="Task"/> representing the operation.</returns>
      Task DeployAsync(bool checkConnection = true, bool logHost = false);
   }
}
