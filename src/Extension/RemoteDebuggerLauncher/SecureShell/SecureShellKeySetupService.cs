// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace RemoteDebuggerLauncher.SecureShell
{
   /// <summary>
   /// Implements the service responsible for registering a SSH public key on a target device.
   /// Implements <see cref="ISecureShellKeySetupService"/>
   /// </summary>
   [Export(typeof(ISecureShellKeySetupService))]
   internal class SecureShellKeySetupService : ISecureShellKeySetupService
   {
      private readonly IVsFacadeFactory factory;

      [ImportingConstructor]
      public SecureShellKeySetupService(IVsFacadeFactory factory)
      {
         this.factory = factory;
      }

      /// <inheritdoc />
      public IOutputPaneWriterService OutputPaneWriter => factory.GetVsShell().GetOutputPaneWriter();

      /// <inheritdoc />
      public IStatusbarService Statusbar => factory.GetVsShell().GetStatusbar();

      /// <inheritdoc />
      public async Task AuthorizeKeyAsync(SecureShellKeySetupSettings settings)
      {
         Statusbar.SetText(Resources.RemoteCommandSetupSshCommandStatusbarProgress);
         OutputPaneWriter.WriteLine(Resources.CommonStartSessionMarker);

         // Step 1: try to authenicate with the supplied public key
         bool success = await TryEstablishConnectionWithKeyAsync(settings,
            Resources.RemoteCommandSetupSshPhase1TryAuthenticatePrivateKeyProgress,
            Resources.RemoteCommandSetupSshPhase1TryAuthenticatePrivateKeySuccess,
            Resources.RemoteCommandSetupSshPhase1TryAuthenticatePrivateKeyFailed);
         if (success)
         {
            return;
         }

         // Step 2: try authenticate with the supplied username/password
         using (var sshClient = await TryEstablishConnectionWithPasswordAsync(settings))
         {
            // Step 3: register the public key with the target
            if (sshClient == null)
            {
               return;
            }

            success = await RegisterPublicKeyAsync(settings, sshClient);
         }

         if (success)
         {
            // Step 4: try to authenicate with the supplied public key
            _ = await TryEstablishConnectionWithKeyAsync(settings,
               Resources.RemoteCommandSetupSshPhase4TryAuthenticatePrivateKeyProgress,
               Resources.RemoteCommandSetupSshPhase4TryAuthenticatePrivateKeySuccess,
               Resources.RemoteCommandSetupSshPhase4TryAuthenticatePrivateKeyFailed);
         }
      }

      private Task<bool> TryEstablishConnectionWithKeyAsync(SecureShellKeySetupSettings settings, string progressText, string successText, string failureText)
      {
         OutputPaneWriter.WriteLine(progressText, settings.UserName, settings.HostName, settings.HostPort);

         try
         {
            var key = new PrivateKeyFile(settings.PrivateKeyFile);
            using (var sshClient = new SshClient(settings.HostName, settings.HostPort, settings.UserName, key))
            {
               sshClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
               sshClient.Connect();
            }

            OutputPaneWriter.WriteLine(successText);
            return Task.FromResult(true);
         }
         catch (SshAuthenticationException expectedException)
         {
            // This is expected
            OutputPaneWriter.WriteLine(failureText, expectedException.Message);
            return Task.FromResult(false);
         }
         catch (Exception ex)
         {
            throw new SecureShellSessionException(ex.Message, ex);
         }
      }

      private Task<SshClient> TryEstablishConnectionWithPasswordAsync(SecureShellKeySetupSettings settings)
      {
         OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshPhase2TryAuthenticatePasswordProgress, settings.UserName, settings.HostName, settings.HostPort);

         SshClient sshClient = null;
         try
         {
            sshClient = new SshClient(settings.HostName, settings.HostPort, settings.UserName, settings.Password);
            sshClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
            sshClient.Connect();

            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshPhase2TryAuthenticatePasswordSuccess);
         }
         catch (SshAuthenticationException expectedException)
         {
            sshClient?.Dispose();
            sshClient = null;
            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshPhase2TryAuthenticatePasswordFailed, expectedException.Message);
         }
         catch (Exception ex)
         {
            sshClient?.Dispose();
            throw new SecureShellSessionException(ex.Message, ex);
         }

         return Task.FromResult(sshClient);
      }

      private Task<bool> RegisterPublicKeyAsync(SecureShellKeySetupSettings settings, SshClient client)
      {
         try
         {
            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshPhase3AddKeyProgress, settings.UserName, settings.HostName, settings.HostPort);

            string publicKeyData = File.ReadAllText(settings.PublicKeyFile).Trim();

            using (var command = client.RunCommand($"mkdir -p ~/.ssh && echo {publicKeyData} >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys"))
            {
               if (command.ExitStatus != 0)
               {
                  OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshPhase3AddKeyFailed, command.Error);
                  return Task.FromResult(false);
               }
            }

            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshPhase3AddKeySuccess);
            return Task.FromResult(true);

         }
         catch (SshException ex)
         {
            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshPhase3AddKeyFailed, ex.Message);
            return Task.FromResult(false);
         }
         catch (Exception ex)
         {
            throw new SecureShellSessionException(ex.Message, ex);
         }
      }
   }
}
