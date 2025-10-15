// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Secure Shell (SSH) session service.
   /// Implements the <see cref="ISecureShellSessionService"/> interface.
   /// Implements the <see cref="IRemoteBulkCopySessionService"/> interface.
   /// </summary>
   /// <seealso cref="ISecureShellSessionService"/>
   /// <seealso cref="IRemoteBulkCopySessionService"/>"/>
   internal class SecureShellSessionService : ISecureShellSessionService, IRemoteBulkCopySessionService
   {
      private readonly SecureShellSessionSettings settings;

      internal SecureShellSessionService(SecureShellSessionSettings settings)
      {
         this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
      }

      /// <inheritdoc/>
      public SecureShellSessionSettings Settings => settings;

      /// <inheritdoc/>
      public Task<string> ExecuteSingleCommandAsync(string commandText)
      {
         return Task.Run(() =>
         {
            try
            {
               using (var client = CreateSshClient())
               {
                  client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
                  client.Connect();
                  using (var command = client.RunCommand(commandText))
                  {
                     return command.Result;
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

      /// <inheritdoc/>
      public Task UploadFolderRecursiveAsync(string localSourcePath, string remoteTargetPath, IOutputPaneWriterService progressOutputPaneWriter = null)
      {
         ThrowIf.ArgumentNullOrEmpty(localSourcePath, nameof(localSourcePath));
         ThrowIf.ArgumentNullOrEmpty(remoteTargetPath, nameof(remoteTargetPath));

         progressOutputPaneWriter?.Write(Resources.RemoteCommandCommonSshTarget, Settings.UserName, Settings.HostName);
         progressOutputPaneWriter?.WriteLine(Resources.RemoteCommandDeployRemoteFolderScpFullStart);

         return Task.Run(() =>
         {
            try
            {
               var sourcePathInfo = new DirectoryInfo(localSourcePath);
               using (var client = CreateScpClient())
               {
                  // for the moment, we assume that the path names does not have any character that have special meaning for a Linux host
                  client.RemotePathTransformation = RemotePathTransformation.None;

                  if (progressOutputPaneWriter != null)
                  {
                     // attach progress output pane writer if available
                     var progressReporter = new SecureShellCopyProgressReporter(progressOutputPaneWriter);
                     client.Uploading += progressReporter.OnUploadFolder;
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

      /// <inheritdoc/>
      public Task UploadFileAsync(string localSourcePath, string remoteTargetPath, IOutputPaneWriterService progressOutputPaneWriter = null)
      {
         ThrowIf.ArgumentNullOrEmpty(localSourcePath, nameof(localSourcePath));
         ThrowIf.ArgumentNullOrEmpty(remoteTargetPath, nameof(remoteTargetPath));

         return Task.Run(() =>
         {
            try
            {
               var sourcePathInfo = new FileInfo(localSourcePath);

               using (var client = CreateScpClient())
               {
                  // for the moment, we assume that the path names does not have any character that have special meaning for a Linux host
                  client.RemotePathTransformation = RemotePathTransformation.None;

                  if (progressOutputPaneWriter != null)
                  {
                     // attach progress output pane writer if available
                     var progressReporter = new SecureShellCopyProgressReporter(progressOutputPaneWriter);
                     client.Uploading += progressReporter.OnUploadFile;
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

      /// <inheritdoc/>
      public Task UploadFileAsync(Stream localStream, string remoteTargetPath)
      {
         ThrowIf.ArgumentNull(localStream, nameof(localStream));
         ThrowIf.ArgumentNullOrEmpty(remoteTargetPath, nameof(remoteTargetPath));

         return Task.Run(() =>
         {
            try
            {
               using (var client = CreateScpClient())
               {
                  // for the moment, we assume that the path names does not have any character that have special meaning for a Linux host
                  client.RemotePathTransformation = RemotePathTransformation.None;

                  client.Connect();
                  client.Upload(localStream, remoteTargetPath);
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

      /// <inheritdoc/>
      public async Task CleanFolderAsync(string remoteTargetPath, bool clean)
      {
         ThrowIf.ArgumentNullOrEmpty(remoteTargetPath, nameof(remoteTargetPath));
         if (clean)
         {
            using (var commandSession = CreateCommandSession())
            {
               _ = await commandSession.ExecuteCommandAsync($"[ -d \"{remoteTargetPath}\" ] && rm -rf \"{remoteTargetPath}\"/*");
               _ = await commandSession.ExecuteCommandAsync($"mkdir -p \"{remoteTargetPath}\"");
            }
         }
      }

      /// <inheritdoc/>
      public ISecureShellSessionCommandingService CreateCommandSession()
      {
         var client = CreateSshClient();
         client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
         return new SecureShellSessionCommandingService(client);
      }

      private SshClient CreateSshClient()
      {
         if (string.IsNullOrWhiteSpace(settings.UserName))
         {
            throw new SecureShellSessionException(ExceptionMessages.SecureShellSessionNoUserName);
         }

         if (!File.Exists(settings.PrivateKeyFile))
         {
            throw new SecureShellSessionException(ExceptionMessages.SecureShellSessionNoPrivateKey);
         }

         var key = new PrivateKeyFile(settings.PrivateKeyFile);

         return new SshClient(settings.HostNameIPv4, settings.HostPort, settings.UserName, key);
      }

      private ScpClient CreateScpClient()
      {
         var key = new PrivateKeyFile(settings.PrivateKeyFile);
         return new ScpClient(settings.HostNameIPv4, settings.HostPort, settings.UserName, key);
      }
   }
}
