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
   /// Interface defining the bulk Remote Copy session service responsible to copy files and folders to the remote host.
   /// </summary>
   internal interface IRemoteBulkCopySessionService : ISecureShellSessionBaseService
   {
      /// <summary>
      /// Uploads the specified folder recursively from the local PC to the remote host.
      /// </summary>
      /// <param name="localSourcePath">The absolute path to the source folder to copy from.</param>
      /// <param name="remoteTargetPath">The absolute path to the remote target path to copy to.</param>
      /// <param name="progressOutputPaneWriter">The optional output pane writer to be used for logging progress.</param>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task UploadFolderRecursiveAsync(string localSourcePath, string remoteTargetPath, IOutputPaneWriterService progressOutputPaneWriter = null);

      /// <summary>
      /// Uploads a single file from the local PC to the remote host.
      /// </summary>
      /// <param name="localFilePath">The absolute path to the source file to copy from.</param>
      /// <param name="remoteFilePath">The absolute path to the remote target file to copy to.</param>
      /// <param name="progressOutputPaneWriter">The optional output pane writer to be used for logging progress.</param>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task UploadFileAsync(string localFilePath, string remoteFilePath, IOutputPaneWriterService progressOutputPaneWriter = null);
   }
}
