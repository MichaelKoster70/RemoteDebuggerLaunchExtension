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
         progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandDeployRemoteFolderScpDeltaStart);

         using (var commands = session.CreateCommandSession())
         {
            // Step 1: Get the user home, needed to normalize path expressions
            var userHome = await GetUserHomeAsync(commands);
            remoteTargetPath = UnixPath.Normalize(remoteTargetPath, userHome);

            // Step 2: ensure that remote tools are installed
            var remoteToolsPath = await InstallRemoteToolsIfNeededAsync(commands, userHome);

            // Step 3: Determine the sha 265 hashes of all files in the remote target directory
            var remoteFileHashes = await GetRemoteFileHashesAsync(commands, remoteTargetPath, remoteToolsPath);

            // Step 4: Compare the local source directory with the remote target directory and determine which files need to be copied or deleted
            var (filesToCopy, filesToDelete) = GetFilesToCopyOrDelete(localSourcePath, remoteFileHashes);

            // Step 5: Delete files that are not needed anymore
            await DeleteRemoteFilesAsync(commands, remoteTargetPath, filesToDelete, progressOutputPaneWriter);

            // Step 6: Copy files that are new or changed
            await CopyRemoteFilesAsync(localSourcePath, remoteTargetPath, filesToCopy, progressOutputPaneWriter);
         }
      }

      /// <inheritdoc/>
      public async Task UploadFileAsync(string localFilePath, string remoteFilePath, IOutputPaneWriterService progressOutputPaneWriter = null)
      {
         ThrowIf.ArgumentNullOrEmpty(localFilePath, nameof(localFilePath));
         ThrowIf.ArgumentNullOrEmpty(remoteFilePath, nameof(remoteFilePath));

         progressOutputPaneWriter?.Write(Resources.RemoteCommandCommonSshTarget, Settings.UserName, Settings.HostName);
         progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandDeployFileProgress, localFilePath, remoteFilePath);

         try
         {
            using (var client = CreateScpClient())
            {
               // We can safely assume that the path names does not have any character that have special meaning for a Linux host
               client.RemotePathTransformation = RemotePathTransformation.None;

               if (progressOutputPaneWriter != null)
               {
                  // attach progress output pane writer if available
                  var progressReporter = new SecureShellCopyProgressReporter(progressOutputPaneWriter);
                  client.Uploading += progressReporter.OnUploadFile;
               }

               client.Connect();

               // Ensure the target directory exists before uploading
               var remoteDirectory = UnixPath.GetDirectoryName(remoteFilePath);
               if (!string.IsNullOrEmpty(remoteDirectory))
               {
                  _ = await session.ExecuteSingleCommandAsync(PackageConstants.LinuxShellCommands.FormatMkDir(remoteDirectory));
               }

               // Upload the file
               using (var fileStream = File.OpenRead(localFilePath))
               {
                  client.Upload(fileStream, remoteFilePath);
               }
            }
         }
         catch (Exception e) when (e is SshException || e is IOException)
         {
            throw new SecureShellSessionException(e.Message, e);
         }

         progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandDeployFileCompletedSuccess, localFilePath, remoteFilePath);
      }

      private async Task<string> InstallRemoteToolsIfNeededAsync(ISecureShellSessionCommandingService commands, string userHome)
      {
         // Step 1a: Get the runtime ID of the remote host
         var runtimeId = await GetRuntimeIdAsync(commands);
         var sourceDirectory = GetRemoteToolsSourceDirectory(runtimeId);

         // Step 1b: get the remote target directory
         var remoteTargetDirectory = GetRemoteToolsTargetDirectory(userHome);

         // Step 2: Check if the remote tools are already installed - compare version.json file contents with expected version
         bool installRemoteTools = true;
         var (exitCode, stdOut, _) = await commands.TryExecuteCommandAsync($"cat {remoteTargetDirectory}/version.json");
         if (exitCode == 0)
         {
            var localVersion = await ReadRemoteToolsVersionFileAsync("version");
            var remoteVersion = JToken.Parse(stdOut)["version"];

            if (JToken.DeepEquals(localVersion, remoteVersion))
            {
               // same version - no need to install
               installRemoteTools = false;
            }
         }

         // step 3: copy the tools if needed
         if (installRemoteTools)
         {
            // create target directory
            _ = await commands.ExecuteCommandAsync(PackageConstants.LinuxShellCommands.FormatMkDir(remoteTargetDirectory));

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

            // set execution flag on the binary files only
            var tools = await ReadRemoteToolsVersionFileAsync("tools");
            if (tools is JArray toolArray)
            {
               foreach (var tool in toolArray)
               {
                  // run chmod +x on the tool
                  var toolPath = $"{remoteTargetDirectory}/{tool}";
                  _ = await commands.ExecuteCommandAsync(PackageConstants.LinuxShellCommands.FormatChmodPlusX(toolPath));
               }
            }
         }

         return remoteTargetDirectory;
      }

      private static Task<string> GetRemoteFileHashesAsync(ISecureShellSessionCommandingService commands, string remoteTargetPath, string remoteToolsPath) => commands.ExecuteCommandAsync($"{remoteToolsPath}/checksum {remoteTargetPath}");

      private static (IReadOnlyList<string> FilesToCopy, IReadOnlyList<string> FilesToDelete) GetFilesToCopyOrDelete (string localSourcePath, string remoteFileHashes)
      {
         var comparer = new DirectoryScannerComparer(localSourcePath);

         return comparer.GetMismatchedFiles(remoteFileHashes);
      }

      private static async Task DeleteRemoteFilesAsync(ISecureShellSessionCommandingService commands, string remoteTargetPath, IReadOnlyList<string> FilesToDelete, IOutputPaneWriterService progressOutputPaneWriter)
      {
         foreach (var file in FilesToDelete)
         {
            // execute rm command for each file
            var absolutePath = UnixPath.Combine(remoteTargetPath, file);
            progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandDeployRemoteFolderScpDeltaDeleteFile, file);
            _ = await commands.ExecuteCommandAsync(PackageConstants.LinuxShellCommands.FormatRm(absolutePath));
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
                  // We can safely assume that the path names does not have any character that have special meaning for a Linux host
                  client.RemotePathTransformation = RemotePathTransformation.None;

                  if (progressOutputPaneWriter != null)
                  {
                     // attach progress output pane writer if available
                     var progressReporter = new SecureShellCopyProgressReporter(progressOutputPaneWriter);
                     client.Uploading += progressReporter.OnUploadFolder;
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

      private static async Task<string> GetUserHomeAsync(ISecureShellSessionCommandingService commands) =>  (await commands.ExecuteCommandAsync("pwd")).Trim('\n');

      private string GetRemoteToolsTargetDirectory(string userHome) => UnixPath.Normalize(configuration.QueryToolsInstallFolderPath(), userHome);


      private static async Task<JToken> ReadRemoteToolsVersionFileAsync(string key)
      {
         using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RemoteDebuggerLauncher.ToolsRemote.version.json"))
         {
            using (var reader = new StreamReader(stream))
            {
               var content = await reader.ReadToEndAsync();
               return JToken.Parse(content)[key];
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
