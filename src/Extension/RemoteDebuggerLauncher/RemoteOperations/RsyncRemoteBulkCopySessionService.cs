// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Rsync-based implementation of <see cref="IRemoteBulkCopySessionService"/>.
   /// https://rsync.samba.org/
   /// Implements the <see cref="IRemoteBulkCopySessionService"/> interface.
   /// </summary>
   internal class RsyncRemoteBulkCopySessionService : IRemoteBulkCopySessionService
   {
      private readonly ISecureShellSessionBaseService session;

      internal RsyncRemoteBulkCopySessionService(ISecureShellSessionBaseService session)
      {
         this.session = session ?? throw new ArgumentNullException(nameof(session));
      }

      /// <inheritdoc/>
      public SecureShellSessionSettings Settings => session.Settings;  
      public Task<string> ExecuteSingleCommandAsync(string commandText) => session.ExecuteSingleCommandAsync(commandText);

      /// <inheritdoc/>
      public ISecureShellSessionCommandingService CreateCommandSession() => session.CreateCommandSession();

      public async Task UploadFolderRecursiveAsync(string localSourcePath, string remoteTargetPath, IOutputPaneWriterService progressOutputPaneWriter = null)
      {
         ThrowIf.ArgumentNullOrEmpty(localSourcePath, nameof(localSourcePath));
         ThrowIf.ArgumentNullOrEmpty(remoteTargetPath, nameof(remoteTargetPath));

         using (var commands = session.CreateCommandSession())
         {
            progressOutputPaneWriter?.Write(Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandInstallDebuggerOnlineCommonProgress);

            // Fail copy if rsync is missing
            await ThrowIfRsyncIsMissingAsync(commands, progressOutputPaneWriter);

         }
      }

      /// <summary>
      /// Checks if Rsync is installed.
      /// </summary>
      /// <param name="commands">The SSH session to use</param>
      /// <returns><c>true</c> if installed; else <c>false</c>.</returns>
      private static async Task ThrowIfRsyncIsMissingAsync(ISecureShellSessionCommandingService commands, IOutputPaneWriterService outputPaneWriter)
      {
         (int exitCode, _, _) = await commands.TryExecuteCommandAsync("command -v rsync");
         if (exitCode != 0)
         {
            outputPaneWriter?.WriteLine(Resources.RemoteCommandCommonFailedRsyncNotInstalled);
            throw new SecureShellSessionException(Resources.RemoteCommandCommonFailedRsyncNotInstalled);
         }
      }
   }
}
