﻿// ----------------------------------------------------------------------------
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

namespace RemoteDebuggerLauncher.SecureShell
{
   /// <summary>
   /// Secure Shell (SSH) session service.
   /// Implements the <see cref="ISecureShellSessionService"/> interface.
   /// </summary>
   /// <seealso cref="ISecureShellSessionService"/>
   internal class SecureShellSessionService : ISecureShellSessionService
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

         return Task.Run(() =>
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
                  long progressBefore = 0;

                  // for the moment, we assume that the path names does not have any character that have special meaning for a Linux host
                  client.RemotePathTransformation = RemotePathTransformation.None;

                  if (progressOutputPaneWriter != null)
                  {
                     // attach progress output pane writer if available
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
                     client.Uploading += async (s, e) =>
                     {
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
