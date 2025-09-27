// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

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

      /// <inheritdoc/>
      public async Task UploadFolderRecursiveAsync(string localSourcePath, string remoteTargetPath, IOutputPaneWriterService progressOutputPaneWriter = null)
      {
         ThrowIf.ArgumentNullOrEmpty(localSourcePath, nameof(localSourcePath));
         ThrowIf.ArgumentNullOrEmpty(remoteTargetPath, nameof(remoteTargetPath));

         using (var commands = session.CreateCommandSession())
         {
            progressOutputPaneWriter?.Write(Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandDeployRemoteFolderRsyncStart);

            // Fail copy if rsync is missing
            await ThrowIfRsyncIsMissingAsync(commands, progressOutputPaneWriter);

            await StartRsyncSessionAsync(localSourcePath, remoteTargetPath, progressOutputPaneWriter);
         }
      }

      /// <summary>
      /// Checks if rsync is installed.
      /// </summary>
      /// <param name="commands">The SSH session to use</param>
      /// <returns><c>true</c> if installed; else <c>false</c>.</returns>
      private static async Task ThrowIfRsyncIsMissingAsync(ISecureShellSessionCommandingService commands, IOutputPaneWriterService outputPaneWriter)
      {
         (int exitCode, _, _) = await commands.TryExecuteCommandAsync("command -v rsync");
         if (exitCode != 0)
         {
            outputPaneWriter?.WriteLine(Resources.RemoteCommandDeployRemoteFolderRsyncFailedNotInstalled);
            throw new SecureShellSessionException(Resources.RemoteCommandDeployRemoteFolderRsyncFailedNotInstalled);
         }
      }

      /// <summary>
      /// Starts the rsync session.
      /// </summary>
      /// <param name="localSourcePath">The local source path.</param>
      /// <param name="remoteTargetPath">The remote target path.</param>
      /// <param name="outputPaneWriter">The output pane writer to use for logging.</param>
      private async Task StartRsyncSessionAsync(string localSourcePath, string remoteTargetPath, IOutputPaneWriterService progressOutputPaneWriter)
      {
         // Step 1: Convert paths to Cygwin style
         localSourcePath = ConvertToAbsoluteCygwinPath(localSourcePath);

         // Step 2: compile command line and launch parameters
         var privateKeyFile = session.Settings.PrivateKeyFile;
         var arguments = $"-rvuz -e 'ssh -i {privateKeyFile}' {localSourcePath} {session.Settings.UserName}@{session.Settings.HostName}:{remoteTargetPath}";

         var rsyncSearchPath = GetRsyncDirectory();

         var startInfo = new ProcessStartInfo ($"rsync.exe", arguments)
         {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
         };

         // Step 3: Set PATH to only the folder where rsync is located
         startInfo.EnvironmentVariables["PATH"] = rsyncSearchPath;

         // Step 4: Start process
         using (var process = Process.Start(startInfo))
         {
            // Method to handle output data
#pragma warning disable VSTHRD100 // Avoid async void methods
            async void OnDataReceived(object _, DataReceivedEventArgs e)
            {
               try
               {
                  var message = e.Data;

                  await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                  {
                     await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                     progressOutputPaneWriter?.WriteLine(message);
                  });
               }
               catch (Exception)
               {
                  // Ignore any exception
               }
            }
#pragma warning restore VSTHRD100 // Avoid async void methods

            process.OutputDataReceived += OnDataReceived;
            process.BeginOutputReadLine();

            var stdError = await process.StandardError.ReadToEndAsync();
            var exitCode = await process.WaitForExitAsync();

            process.OutputDataReceived -= OnDataReceived;

            if (exitCode > 0)
            {
               progressOutputPaneWriter?.WriteLine(stdError);
               throw new RemoteDebuggerLauncherException(string.Format(Resources.RemoteCommandDeployRemoteFolderRsyncFailedExitCode, exitCode));
            }
         }
      }

      /// <summary>
      /// Gets the directory where rsync is located.
      /// </summary>
      /// <returns>The directory path of the current assembly.</returns>
      private static string GetRsyncDirectory()
      {
         var assemblyLocation = Assembly.GetExecutingAssembly().Location;
         return Path.Combine(Path.GetDirectoryName(assemblyLocation), "ToolsExternal\\bin");
      }

      /// <summary>
      /// Converts the supplied path to an absolute Cygwin style path.
      /// </summary>
      /// <param name="path">The path to convert.</param>
      /// <returns>The Cygwin-style path.</returns>
      private static string ConvertToAbsoluteCygwinPath(string absolutePath)
      {
         // ensure we get an absolute path
         var rootPath = Path.GetPathRoot(absolutePath);
         if (string.IsNullOrWhiteSpace(rootPath))
         {
            throw new SecureShellSessionException(Resources.RemoteCommandDeployRemoteFolderRsyncFailedNoAbsolutePath);
         }

         var driveLetter = rootPath.ToLower().Substring(0, 1);
         var directory = Path.GetRelativePath(rootPath, Path.GetDirectoryName(absolutePath)).Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
         return $"/cygdrive/{driveLetter}/{directory}";
      }
   }
}
