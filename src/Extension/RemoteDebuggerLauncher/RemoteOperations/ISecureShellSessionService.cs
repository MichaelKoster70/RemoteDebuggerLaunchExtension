// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Interface defining the Secure Shell (SSH) session service.
   /// </summary>
   internal interface ISecureShellSessionService : ISecureShellSessionBaseService
   {
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
   }
}
