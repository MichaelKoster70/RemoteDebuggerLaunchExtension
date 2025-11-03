// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
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
      private readonly ISecureShellKeyPassphraseService passphraseService;

      internal SecureShellSessionService(SecureShellSessionSettings settings, ISecureShellKeyPassphraseService passphraseService)
      {
         this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
         this.passphraseService = passphraseService ?? throw new ArgumentNullException(nameof(passphraseService));
      }

      /// <inheritdoc/>
      public SecureShellSessionSettings Settings => settings;

      /// <inheritdoc/>
      public Task<string> ExecuteSingleCommandAsync(string commandText)
      {
         return Task.Run(async () =>
         {
            try
            {
               using (var client = await CreateSshClientAsync())
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

         return Task.Run(async () =>
         {
            try
            {
               var sourcePathInfo = new DirectoryInfo(localSourcePath);
               using (var client = await CreateScpClientAsync())
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

         return Task.Run(async () =>
         {
            try
            {
               var sourcePathInfo = new FileInfo(localSourcePath);

               using (var client = await CreateScpClientAsync())
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

         return Task.Run(async () =>
         {
            try
            {
               using (var client = await CreateScpClientAsync())
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
            using (var commandSession = await CreateCommandSessionAsync())
            {
               _ = await commandSession.ExecuteCommandAsync($"[ -d \"{remoteTargetPath}\" ] && rm -rf \"{remoteTargetPath}\"/*");
               _ = await commandSession.ExecuteCommandAsync($"mkdir -p \"{remoteTargetPath}\"");
            }
         }
      }

      /// <inheritdoc/>
      public async Task<ISecureShellSessionCommandingService> CreateCommandSessionAsync()
      {
         var client = await CreateSshClientAsync();
         client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
         return new SecureShellSessionCommandingService(client);
      }

      private async Task<SshClient> CreateSshClientAsync()
      {
         if (string.IsNullOrWhiteSpace(settings.UserName))
         {
            throw new SecureShellSessionException(ExceptionMessages.SecureShellSessionNoUserName);
         }

         if (!File.Exists(settings.PrivateKeyFile))
         {
            throw new SecureShellSessionException(ExceptionMessages.SecureShellSessionNoPrivateKey);
         }

         var key = await CreatePrivateKeyFileAsync(settings.PrivateKeyFile);

         return new SshClient(settings.HostNameIPv4, settings.HostPort, settings.UserName, key);
      }

      private async Task<ScpClient> CreateScpClientAsync()
      {
         var key = await CreatePrivateKeyFileAsync(settings.PrivateKeyFile);
         return new ScpClient(settings.HostNameIPv4, settings.HostPort, settings.UserName, key);
      }

      private async Task<PrivateKeyFile> CreatePrivateKeyFileAsync(string privateKeyFilePath)
      {
         // First, check if we have a cached passphrase
         if (passphraseService.TryGet(privateKeyFilePath, out var passphrase))
         {
            try
            {
               return new PrivateKeyFile(privateKeyFilePath, passphrase);
            }
            catch (SshException)
            {
               // Cached passphrase is wrong, clear it and continue
               passphraseService.Clear(privateKeyFilePath);
            }
         }

         // Try without passphrase first
         try
         {
            return new PrivateKeyFile(privateKeyFilePath);
         }
         catch (SshException)
         {
            // Assume the key is encrypted, proceed to passphrase handling

            try
            {
               await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
               if (passphraseService.Prompt(privateKeyFilePath, out passphrase))
               {
                  return new PrivateKeyFile(privateKeyFilePath, passphrase);
               }
               else
               {
                  throw new SecureShellSessionException(ExceptionMessages.SecureShellSessionPassphraseRequired);
               }
            }
            finally
            {
               await TaskScheduler.Default;
            }
         }
      }
   }
}
