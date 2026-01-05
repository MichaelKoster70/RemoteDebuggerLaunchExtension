// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using RemoteDebuggerLauncher.Infrastructure;
using RemoteDebuggerLauncher.Logging;
using RemoteDebuggerLauncher.Shared;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Implements the service responsible for registering a SSH public key on a target device.
   /// Implements <see cref="ISecureShellKeySetupService"/>
   /// </summary>
   [Export(typeof(ISecureShellKeySetupService))]
   internal class SecureShellKeySetupService : ISecureShellKeySetupService
   {

      private readonly IVsFacadeFactory factory;
      private readonly ILogger logger;

      [ImportingConstructor]
      public SecureShellKeySetupService(IVsFacadeFactory factory, ILoggerFactory loggerFactory)
      {
         this.factory = factory;
         logger = loggerFactory.CreateLogger(nameof(SecureShellKeySetupService));
      }

      /// <inheritdoc />
      public IOutputPaneWriterService OutputPaneWriter => factory.GetVsShell().GetOutputPaneWriter();

      /// <inheritdoc />
      public IStatusbarService Statusbar => factory.GetVsShell().GetStatusbar();

      /// <inheritdoc />
      public async Task RegisterServerFingerprintAsync(SecureShellKeySetupSettings settings)
      {
         logger.LogInformation($"Starting server fingerprint registration for {settings.UserName}@{settings.HostName}:{settings.HostPort}");
         Statusbar.SetText(Resources.RemoteCommandSetupSshCommandStatusbarScanProgress);
         OutputPaneWriter.WriteLine(Resources.CommonStartSessionMarker);

         // Try 1: Use ssh-keyscan to get the server fingerprint and add it to the known_hosts file
         var (success, keyscanStdError) = await RegisterServerFingerprintWithKeyScanAsync(settings);
         if (!success)
         {
            logger.LogWarning("Failed to register server fingerprint using ssh-keyscan, trying interactive connection");
            // Try 2: Establish an interactive SSH connection to the server to get the fingerprint
            success = await RegisterServerFingerprintWithConnectionAsync(settings);
         }

         if (!success)
         {
            logger.LogError($"Failed to register server fingerprint for {settings.UserName}@{settings.HostName}:{settings.HostPort}");
            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintFailed1, settings.UserName, settings.HostName, settings.HostPort);
            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintFailed2);
            OutputPaneWriter.WriteLine(keyscanStdError);
            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintFailed3);
         }
         else
         {
            logger.LogInformation("Successfully registered server fingerprint");
         }
      }

      /// <inheritdoc />
      public async Task AuthorizeKeyAsync(SecureShellKeySetupSettings settings)
      {
         logger.LogInformation($"Starting SSH key authorization for {settings.UserName}@{settings.HostName}:{settings.HostPort}");
         Statusbar.SetText(Resources.RemoteCommandSetupSshCommandStatusbarAuthorizeProgress);
         OutputPaneWriter.WriteLine(Resources.CommonStartSessionMarker);

         // Step 1: try to authenticate with the supplied public key
         bool success = await TryEstablishConnectionWithKeyAsync(settings,
            Resources.RemoteCommandSetupSshPhase1TryAuthenticatePrivateKeyProgress,
            Resources.RemoteCommandSetupSshPhase1TryAuthenticatePrivateKeySuccess,
            Resources.RemoteCommandSetupSshPhase1TryAuthenticatePrivateKeyFailed);
         if (success)
         {
            logger.LogInformation("SSH key already authorized, no action needed");
            return;
         }

         logger.LogDebug("Private key authentication failed, trying password authentication");
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
            // Step 4: try to authenticate with the supplied public key
            _ = await TryEstablishConnectionWithKeyAsync(settings,
               Resources.RemoteCommandSetupSshPhase4TryAuthenticatePrivateKeyProgress,
               Resources.RemoteCommandSetupSshPhase4TryAuthenticatePrivateKeySuccess,
               Resources.RemoteCommandSetupSshPhase4TryAuthenticatePrivateKeyFailed);
         }
      }

      private async Task<(bool, string)> RegisterServerFingerprintWithKeyScanAsync(SecureShellKeySetupSettings settings)
      {
         var defaultKeysFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), PackageConstants.SecureShell.DefaultKeyPairFolder);
         var knownHostsFilePath = Path.Combine(defaultKeysFolder, PackageConstants.SecureShell.DefaultKnownHostsFileName);

         var arguments = string.Format(PackageConstants.SecureShell.KeyScanArguments, settings.HostName, settings.HostPort, settings.ForceIPv4 ? PackageConstants.SecureShell.SshForceIPv4 : string.Empty);
         var startInfo = new ProcessStartInfo(PackageConstants.SecureShell.KeyScanExecutable, arguments)
         {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
         };

         OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintScan1, settings.UserName, settings.HostName, settings.HostPort);
         OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintScan2, arguments);

         using (var process = Process.Start(startInfo))
         {
            var stdOutput = await process.StandardOutput.ReadToEndAsync();
            var stdError = await process.StandardError.ReadToEndAsync();
            var exitCode = await process.WaitForExitAsync();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (exitCode == 0)
            {
               if (FileHelper.ContainsText(knownHostsFilePath, settings.HostName))
               {
                  OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintSkip, settings.UserName, settings.HostName, settings.HostPort);
               }
               else
               {
                  OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintAdd, settings.UserName, settings.HostName, settings.HostPort);
                  File.AppendAllText(knownHostsFilePath, stdOutput);
               }
            }

            return (exitCode == 0, stdError);
         }
      }

      private async Task<bool> RegisterServerFingerprintWithConnectionAsync(SecureShellKeySetupSettings settings)
      {
         // Time to wait for initial output from SSH process
         const double SshInitialOutputDelaySeconds = 5d;

         // Overall timeout for the SSH interaction
         const double SshOverallTimeoutSeconds = 10d;

         // Known prompts and markers
         const string SshFingerprintPrompt = "Are you sure you want to continue connecting";
         const string SshPasswordPrompt = "password:";
         const string SshDoneMarker = "DONE";

         // open a pseudo console windows to run the ssh command
         var arguments = string.Format(PackageConstants.SecureShell.SshArguments, settings.UserName, settings.HostName, settings.HostPort, settings.PrivateKeyFile, settings.ForceIPv4 ? PackageConstants.SecureShell.SshForceIPv4 : string.Empty);
         var startInfo = new ProcessStartInfo(PackageConstants.SecureShell.SshExecutable, arguments)
         {
            CreateNoWindow = false,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
         };

         OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintSsh1, settings.UserName, settings.HostName, settings.HostPort);
         OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintSsh2, arguments);

         using (var process = PseudoConsoleProcess.Start(startInfo))
         {
            var stringBuilder = new StringBuilder();
            var inactivityWatch = Stopwatch.StartNew();

            // Wait a couple of seconds for initial output. The process may take some time to start.
            await Task.Delay(TimeSpan.FromSeconds(SshInitialOutputDelaySeconds));

            // Continuously read characters while available
            while (true)
            {
               // Read any available characters
               while (process.StandardOutput.Peek() > -1)
               {
                  var ch = (char)process.StandardOutput.Read();

                  _ = stringBuilder.Append(ch);

                  inactivityWatch.Restart();
               }

               // Check for known prompts
               var stdOutput = stringBuilder.ToStringStripAnsi();

               if (stdOutput.Contains(SshFingerprintPrompt))
               {
                  // We have seen the fingerprint prompt -> confirm it
                  await process.StandardInput.WriteLineAsync("yes");
                  _ = stringBuilder.Clear();
                  inactivityWatch.Restart();

                  // report progress
                  OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintSsh3, stdOutput);
               }
               else if (stringBuilder.ToString().Contains(SshPasswordPrompt))
               {
                  // We have seen the password prompt -> abort the connection & exit
                  OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintSsh4);

                  await process.StandardInput.FlushAsync();
                  return true;
               }
               else if (stdOutput.Contains(SshDoneMarker))
               {
                  // We have seen the success marker => exit
                  return true;
               }

               // If no new data for an extensive period of time, terminate
               if (inactivityWatch.Elapsed >= TimeSpan.FromSeconds(SshOverallTimeoutSeconds))
               {
                  // Close stdin to signal EOF and dispose to tear down the pseudo console
                  OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshScanProgressFingerprintSsh5);

                  await process.StandardInput.FlushAsync();

                  return false;
               }

               // Small delay to avoid busy loop
               await Task.Delay(50).ConfigureAwait(false);
            } ///end while(true)
         }
      }

      private Task<bool> TryEstablishConnectionWithKeyAsync(SecureShellKeySetupSettings settings, string progressText, string successText, string failureText)
      {
         OutputPaneWriter.WriteLine(progressText, settings.UserName, settings.HostName, settings.HostPort);

         try
         {
            var key = new PrivateKeyFile(settings.PrivateKeyFile);
            using (var sshClient = new SshClient(settings.HostNameIPv4, settings.HostPort, settings.UserName, key))
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
            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshConnectionFailed, ex.Message);
            throw new SecureShellSessionException(ex.Message, ex);
         }
      }

      private Task<SshClient> TryEstablishConnectionWithPasswordAsync(SecureShellKeySetupSettings settings)
      {
         OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshPhase2TryAuthenticatePasswordProgress, settings.UserName, settings.HostName, settings.HostPort);

         SshClient sshClient = null;
         try
         {
            sshClient = new SshClient(settings.HostNameIPv4, settings.HostPort, settings.UserName, settings.Password);
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
            OutputPaneWriter.WriteLine(Resources.RemoteCommandSetupSshPhase3AddKeyProgress, settings.UserName, settings.HostNameIPv4, settings.HostPort);

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
