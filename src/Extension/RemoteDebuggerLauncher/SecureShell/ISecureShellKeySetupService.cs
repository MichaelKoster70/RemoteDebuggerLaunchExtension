// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;

namespace RemoteDebuggerLauncher.SecureShell
{
   /// <summary>
   /// Interface defining a service responsible for registering a SSH public key on a target device.
   /// </summary>
   internal interface ISecureShellKeySetupService
   {
      /// <summary>
      /// Gets the Output Pane Writer.
      /// </summary>
      IOutputPaneWriterService OutputPaneWriter { get; }

      /// <summary>
      /// Gets the VS Statusbar.
      /// </summary>
      IStatusbarService Statusbar { get; }

      /// <summary>
      /// Registers the SSH server fingerprints in the local 'known_keys' file
      /// </summary>
      /// <param name="settings">The settings to use.</param>
      /// <returns>The <see cref="Task"/> representing the operation</returns>
      Task RegisterServerFingerprintAsync(SecureShellKeySetupSettings settings);

      /// <summary>
      /// Authorizes the supplied key on the target device.
      /// </summary>
      /// <param name="settings">The settings to use.</param>
      /// <returns>The <see cref="Task"/> representing the operation</returns>
      Task AuthorizeKeyAsync(SecureShellKeySetupSettings settings);
   }
}
