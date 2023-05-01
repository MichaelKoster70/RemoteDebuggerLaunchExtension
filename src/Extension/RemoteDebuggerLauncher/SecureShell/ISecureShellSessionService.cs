// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher.SecureShell
{
   /// <summary>
   /// Interface defining the Secure Shell (SSH) session service.
   /// </summary>
   internal interface ISecureShellSessionService
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
      /// Uploads the specified folder recursively from the local PC to the remote host using SCP.
      /// </summary>
      /// <param name="localSourcePath">The absolute path to the source folder to copy from.</param>
      /// <param name="remoteTargetPath">The absolute path to the remote target path to copy to.</param>
      /// <param name="progressOutputPaneWriter">The optional output pane writer to be used for logging progress.</param>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task UploadFolderRecursiveAsync(string localSourcePath, string remoteTargetPath, IOutputPaneWriterService progressOutputPaneWriter = null);

      /// <summary>
      /// Uploads the specified file from the local PC to the remote host using SCP.
      /// </summary>
      /// <param name="localSourcePath">The absolute path to the source file to copy.</param>
      /// <param name="remoteTargetPath">The absolute path to the remote target path to copy to.</param>
      /// <param name="progressOutputPaneWriter">The optional output pane writer to be used for logging progress.</param>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task UploadFileAsync(string localSourcePath, string remoteTargetPath, IOutputPaneWriterService progressOutputPaneWriter = null);

      /// <summary>
      /// Uploads the specified file from the local PC to the remote host using SCP.
      /// </summary>
      /// <param name="localStream">The stream to upload.</param>
      /// <param name="remoteTargetPath">The absolute path to the remote target path to copy to.</param>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task UploadFileAsync(Stream localStream, string remoteTargetPath);

      /// <summary>
      /// Create a new SSH commanding session.
      /// </summary>
      /// <returns>The <see cref="ISecureShellSessionCommandingService"/> session instance.</returns>
      ISecureShellSessionCommandingService CreateCommandSession();
   }
}
