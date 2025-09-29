// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json.Linq;
using RemoteDebuggerLauncher.CheckSum;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Bulk copy implementation using SCP to transfer only changed files.
   /// Implements the <see cref="IRemoteBulkCopySessionService"/> interface.
   /// </summary>
   internal class SecureShellRemoteBulkCopyDeltaSessionService : IRemoteBulkCopySessionService
   {
      private readonly ISecureShellSessionBaseService session;
      private readonly ConfigurationAggregator configuration;

      internal SecureShellRemoteBulkCopyDeltaSessionService(ISecureShellSessionBaseService session, ConfigurationAggregator configuration)
      {
         this.session = session ?? throw new ArgumentNullException(nameof(session));
         this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

         progressOutputPaneWriter?.Write(Resources.RemoteCommandCommonSshTarget, Settings.UserName, Settings.HostName);
         progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandDeployRemoteFolderScpFullStart);

         // Step 1: ensure that remote tools are installed
         var remoteToolsPath = await InstallRemoteToolsIfNeededAsync();

         // Step 2: Determine the sha 265 hashes of all files in the remote target directory
         var remoteFileHashes = await GetRemoteFileHashesAsync(remoteTargetPath, remoteToolsPath);

         // Step 3: Compare the local source directory with the remote target directory and determine which files need to be copied or deleted
         var (filesToCopy, filesToDelete) = GetFileToCopyOrDelete(localSourcePath, remoteFileHashes);

         // Step 4: Delete files that are not needed anymore
         await DeleteRemoteFilesAsync(remoteTargetPath, filesToDelete, progressOutputPaneWriter);
         
         // Step 5: Copy files that are new or changed
         await CopyRemoteFilesAsync(localSourcePath, remoteTargetPath, filesToCopy, progressOutputPaneWriter);
      }

      private async Task<string> InstallRemoteToolsIfNeededAsync()
      {
         // get the remote tools directory
         using (var commands = session.CreateCommandSession())
         {
            // Step 1a: Get the runtime ID of the remote host
            var runtimeId = await GetRuntimeIdAsync(commands);
            var sourceDirectory = GetRemoteToolsSourceDirectory(runtimeId);

            // Step 1b: get the remote target directory
            var remoteTargetDirectory = await GetRemoteToolsTargetDirectoryAsync(commands);

            // Step 2: Check if the remote tools are already installed - compare version.json file contents with expected version
            bool installRemoteTools = true;
            var (exitCode, stdOut, _) = await commands.TryExecuteCommandAsync($"cat {remoteTargetDirectory}/version.json");
            if (exitCode == 0)
            {
               var localVersionContent = await ReadRemoteToolsVersionFileAsync();
               var localVersion = JToken.Parse(localVersionContent);
               var remoteVersion = JToken.Parse(stdOut);

               if (JToken.DeepEquals(localVersion, remoteVersion))
               {
                  // same version - no need to install
                  installRemoteTools = false;
               }
            }

            // step 3: copy the tools if needed
            if (installRemoteTools)
            {
               // copy the tools
               using (var client = CreateScpClient())
               {
                  try
                  {
                     client.Connect();
                     client.Upload(sourceDirectory, remoteTargetDirectory);
                  }
                  catch (SshException e)
                  {
                     throw new SecureShellSessionException(e.Message, e);
                  }
                  catch (InvalidOperationException e)
                  {
                     throw new SecureShellSessionException(e.Message, e);
                  }
               }

               // set execution flag
               _ = await commands.ExecuteCommandAsync($"chmod +x {remoteTargetDirectory}/*");
            }

            return remoteTargetDirectory;
         }
      }

      private async Task<string> GetRemoteFileHashesAsync(string remoteTargetPath, string remoteToolsPath)
      {
         using (var commands = session.CreateCommandSession())
         {
            var fileHashes = await commands.ExecuteCommandAsync($"{remoteToolsPath}/checksum {remoteTargetPath}");
            return fileHashes;
         }
      }

      private static (IReadOnlyList<string> FilesToCopy, IReadOnlyList<string> FilesToDelete) GetFileToCopyOrDelete (string localSourcePath, string remoteFileHashes)
      {
         var comparer = new DirectoryScannerComparer(localSourcePath);

         return comparer.GetMismatchedFiles(remoteFileHashes);
      }

      private async Task DeleteRemoteFilesAsync(string remoteTargetPath, IReadOnlyList<string> FilesToDelete, IOutputPaneWriterService progressOutputPaneWriter)
      {
         using (var commands = session.CreateCommandSession())
         {
            foreach (var file in FilesToDelete)
            {
               var absolutePath = UnixPath.Combine(remoteTargetPath, file);
               progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandDeployRemoteFolderScpDeltaDeleteFile, file);
               _ = await commands.ExecuteCommandAsync($"rm {absolutePath}");
            }
         }
      }

      private async Task CopyRemoteFilesAsync(string localSourcePath, string remoteTargetPath, IReadOnlyList<string> FilesToCopy, IOutputPaneWriterService progressOutputPaneWriter)
      {
         await Task.Run(() =>
         {
            try
            {
               using (var client = CreateScpClient())
               {
                  long progressBefore = 0;
                  string filenameBefore = string.Empty;

                  // for the moment, we assume that the path names does not have any character that have special meaning for a Linux host
                  client.RemotePathTransformation = RemotePathTransformation.None;

                  if (progressOutputPaneWriter != null)
                  {
                     // attach progress output pane writer if available
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
                     client.Uploading += async (s, e) =>
                     {
                        if (filenameBefore == e.Filename)
                        {
                           // same file
                           if (e.Uploaded == e.Size)
                           {
                              await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                              progressOutputPaneWriter.WriteLine(Resources.RemoteCommandUploadOutputPaneDone);
                           }
                           else
                           {
                              long progressNow = 100 * e.Uploaded / e.Size;
                              if ((progressNow > progressBefore) && (progressNow % 10 == 0))
                              {
                                 progressBefore = progressNow;
                                 await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                                 progressOutputPaneWriter.Write(Resources.RemoteCommandUploadOutputPaneProgress);
                              }
                           }
                        }
                        else
                        {
                           // new file
                           filenameBefore = e.Filename;
                           progressBefore = 0;
                           await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                           progressOutputPaneWriter.Write(Resources.RemoteCommandUploadOutputPaneStart, e.Filename);

                           if (e.Uploaded == e.Size)
                           {
                              progressOutputPaneWriter.WriteLine(Resources.RemoteCommandUploadOutputPaneDone);
                           }
                        }

                        await TaskScheduler.Default;
                     };
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
                  }

                  client.Connect();
                  foreach (var file in FilesToCopy)
                  {
                     var sourcePathInfo = new FileInfo(Path.Combine(localSourcePath, file));
                     var absoluteRemoteTargetPath = UnixPath.Combine(remoteTargetPath, file);
                     client.Upload(sourcePathInfo, absoluteRemoteTargetPath);
                  }
               }
            }
            catch (SshException e)
            {
               throw new SecureShellSessionException(e.Message, e);
            }
            catch (InvalidOperationException e)
            {
               throw new SecureShellSessionException(e.Message, e);
            }
         });
      }

      private static DirectoryInfo GetRemoteToolsSourceDirectory(string runtimeId)
      {
         var assemblyLocation = Assembly.GetExecutingAssembly().Location;
         return new DirectoryInfo(Path.Combine(Path.GetDirectoryName(assemblyLocation), "ToolsRemote", runtimeId));
      }

      private static async Task<string> GetRuntimeIdAsync(ISecureShellSessionCommandingService commands)
      {
         string runtimeId;
         var cpuArchitecture = (await commands.ExecuteCommandAsync("uname -m")).Trim('\n');
         switch (cpuArchitecture)
         {
            case "armv7l":
               runtimeId = "linux-arm";
               break;
            case "aarch64":
               runtimeId = "linux-arm64";
               break;
            case "x86_64":
               runtimeId = "linux-x64";
               break;
            default:
               throw new RemoteDebuggerLauncherException("Unknown CPU architecture");
         }

         return runtimeId;
      }

      private async Task<string> GetRemoteToolsTargetDirectoryAsync(ISecureShellSessionCommandingService commands)
      {
         var userHome = (await commands.ExecuteCommandAsync("pwd")).Trim('\n');

         return UnixPath.Normalize(configuration.QueryToolsInstallFolderPath(), userHome);
      }

      private static Task<string> ReadRemoteToolsVersionFileAsync()
      {
         using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RemoteDebuggerLauncher.Resources.version.json"))
         {
            using (var reader = new StreamReader(stream))
            {
               return reader.ReadToEndAsync();
            }
         }
      }

      private ScpClient CreateScpClient()
      {
         var key = new PrivateKeyFile(session.Settings.PrivateKeyFile);
         return new ScpClient(session.Settings.HostNameIPv4, session.Settings.HostPort, session.Settings.UserName, key);
      }
   }
}
