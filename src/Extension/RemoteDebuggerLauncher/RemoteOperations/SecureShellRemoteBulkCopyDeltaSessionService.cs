// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
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

      internal SecureShellRemoteBulkCopyDeltaSessionService(ISecureShellSessionBaseService session)
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

         progressOutputPaneWriter?.Write(Resources.RemoteCommandCommonSshTarget, Settings.UserName, Settings.HostName);
         progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandDeployRemoteFolderScpFullStart);

         // ensure that remote tools are installed
         await InstallRemoteToolsIfNeededAsync();

         await Task.Run(() =>
         {
            try
            {
               var sourcePathInfo = new DirectoryInfo(localSourcePath);
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
                  client.Upload(sourcePathInfo, remoteTargetPath);
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

      private async Task InstallRemoteToolsIfNeededAsync()
      {
         using (var commands = session.CreateCommandSession())
         {
            var runtimeId = await GetRuntimeIdAsync(commands);
            var sourceDirectory = GetRemoteToolsDirectory(runtimeId);

            // copy the tools
            using (var client = CreateScpClient())
            {
               try
               {
                  client.Connect();
                  foreach (var file in sourceDirectory.GetFiles())
                  {
                     client.Upload(file, $"~/{file.Name}");
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
               }
               // set execution flag
               _ = await commands.ExecuteCommandAsync("chmod +x ~/rdl/*");

            }
      }

      private static DirectoryInfo GetRemoteToolsDirectory(string runtimeId)
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
      private ScpClient CreateScpClient()
      {
         var key = new PrivateKeyFile(session.Settings.PrivateKeyFile);
         return new ScpClient(session.Settings.HostNameIPv4, session.Settings.HostPort, session.Settings.UserName, key);
      }
   }
}
