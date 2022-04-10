﻿// ----------------------------------------------------------------------------
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

namespace RemoteDebuggerLauncher
{
   internal class SecureShellSession
   {
      private readonly SecureShellSessionSettings settings;

      internal SecureShellSession(SecureShellSessionSettings settings)
      {
         this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
      }

      /// <summary>
      /// Gets the session settings.
      /// </summary>
      /// <value>The settings.</value>
      public SecureShellSessionSettings Settings => settings;

      /// <summary>
      /// Creates an <see cref="SecureShellSession"/> with settings read from the supplied configuration.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <returns>An <see cref="SecureShellSession"/> instance</returns>
      /// <exception cref="ArgumentNullException">configurationAggregator is null.</exception>
      public static SecureShellSession Create(ConfigurationAggregator configurationAggregator)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));
         var settings = SecureShellSessionSettings.Create(configurationAggregator);

         return new SecureShellSession(settings);
      }

      /// <summary>
      /// Executes a single SSH command asynchronous.
      /// </summary>
      /// <param name="commandText">The command text.</param>
      /// <returns>A <see cref="Task{String}"/> holding the command response.</returns>
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

      public Task UploadFolderRecursiveAsync (string localSourcePath,  string remoteTargetPath)
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
                  // for the moment, we assume that the path names does not have any character that have special meaning for a Linux host
                  client.RemotePathTransformation = RemotePathTransformation.None;
                  client.Connect();
                  client.Upload(sourcePathInfo, remoteTargetPath);
                  client.Uploading += Client_Uploading;
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

      private void Client_Uploading(object sender, ScpUploadEventArgs e)
      {
      }

      public ISecureShellSessionCommanding CreateCommandSession()
      {
         var client = CreateSshClient();
         client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
         return new SecureShellSessionCommanding(client);
      }

      private SshClient CreateSshClient()
      {
         switch (settings.Authentication)
         {
            case AuthenticationKind.Password:
               return new SshClient(settings.HostName, settings.UserName, "");
            case AuthenticationKind.PrivateKey:
               var key = new PrivateKeyFile(settings.PrivateKeyFile);
               return new SshClient(settings.HostName, settings.UserName, key);
            default:
               throw new InvalidOperationException("unsupported authentication");
         }
      }

      private ScpClient CreateScpClient()
      {
         switch (settings.Authentication)
         {
            case AuthenticationKind.Password:
               return new ScpClient(settings.HostName, settings.UserName, "");
            case AuthenticationKind.PrivateKey:
               var key = new PrivateKeyFile(settings.PrivateKeyFile);
               return new ScpClient(settings.HostName, settings.UserName, key);
            default:
               throw new InvalidOperationException("unsupported authentication");
         }
      }
   }
}