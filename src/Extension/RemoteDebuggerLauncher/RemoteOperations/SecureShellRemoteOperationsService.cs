// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Build.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS;
using RemoteDebuggerLauncher.PowerShellHost;
using RemoteDebuggerLauncher.Shared;
using Constants = RemoteDebuggerLauncher.Shared.Constants;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Service for the high level operations performed on the remote device.
   /// Implements the <see cref="ISecureShellRemoteOperationsService"/> interface.
   /// </summary>
   /// <seealso cref="ISecureShellRemoteOperationsService" />
   internal class SecureShellRemoteOperationsService : ISecureShellRemoteOperationsService
   {
      private readonly ConfigurationAggregator configurationAggregator;
      private readonly ISecureShellSessionService session;
      private readonly IRemoteBulkCopySessionService bulkCopy;
      private readonly IDebugTokenReplacer tokenReplacer;
      private readonly IOutputPaneWriterService outputPaneWriter;
      private readonly IStatusbarService statusbar;
      private readonly ILogger logger;

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellRemoteOperations" /> class.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <param name="session">The SSH session to use.</param>
      /// <param name="bulkCopy">The bulk copy session service to use.</param>
      /// <param name="outputPaneWriter">The output pane writer service instance to use.</param>
      /// <param name="statusbar">Optional statusbar service to report progress.</param>
      internal SecureShellRemoteOperationsService(ConfigurationAggregator configurationAggregator, ISecureShellSessionService session, IRemoteBulkCopySessionService bulkCopy, IDebugTokenReplacer tokenReplacer, IOutputPaneWriterService outputPaneWriter, IStatusbarService statusbar, ILoggerFactory loggerFactory)
      {
         this.configurationAggregator = configurationAggregator;
         this.session = session;
         this.bulkCopy = bulkCopy;
         this.tokenReplacer = tokenReplacer;
         this.outputPaneWriter = outputPaneWriter;
         this.statusbar = statusbar;
         logger = loggerFactory.CreateLogger<SecureShellRemoteOperationsService>();
      }

      /// <inheritdoc />
      public bool LogHost { get; set; }

      /// <inheritdoc />
      public async Task CheckConnectionThrowAsync(bool logProgress = true)
      {
         try
         {
            if (session.Settings.IsHostPortDefault)
            {
               outputPaneWriter.Write(logProgress, Resources.RemoteCommandCheckConnectionOutputPaneConnectingTo, session.Settings.UserName, session.Settings.HostName);
            }
            else
            {
               outputPaneWriter.Write(logProgress, Resources.RemoteCommandCheckConnectionOutputPaneConnectingToWithPort, session.Settings.UserName, session.Settings.HostName, session.Settings.HostPort);
            }
            statusbar?.SetText(logProgress, Resources.RemoteCommandCheckConnectionStatusbarProgress, session.Settings.HostName);

            _ = await session.ExecuteSingleCommandAsync("hello echo");

            outputPaneWriter.WriteLine(logProgress, Resources.RemoteCommandCommonSuccess);
            statusbar?.SetText(logProgress, Resources.RemoteCommandCheckConnectionStatusbarCompletedSuccess, session.Settings.HostName);
         }
         catch (Exception ex)
         {
            // whatever exception is thrown indicates a problem
            outputPaneWriter.WriteLine(logProgress, Resources.RemoteCommandCommonFailed, ex.Message);
            throw new RemoteDebuggerLauncherException($"Cannot connect to {session.Settings.UserName}@{session.Settings.HostName} : {ex.Message}", ex);
         }
      }

      /// <inheritdoc />
      public async Task<string> QueryUserHomeDirectoryAsync()
      {
         try
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.Write("Query User Home: ");
            var result = (await session.ExecuteSingleCommandAsync("pwd")).Trim('\n');
            outputPaneWriter.WriteLine(result);
            return result;
         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.WriteLine(Resources.RemoteCommandCommonFailed, ex.Message);
            return string.Empty;
         }
      }

      #region VS Code Debugger
      /// <inheritdoc />
      public async Task<bool> TryInstallVsDbgOnlineAsync(string version = Constants.Debugger.VersionLatest)
      {
         try
         {
            using (var commands = session.CreateCommandSession())
            {
               outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDebuggerOnlineCommonProgress);
               statusbar?.SetText(Resources.RemoteCommandInstallDebuggerOnlineCommonProgress);

               var debuggerInstallPath = configurationAggregator.QueryDebuggerInstallFolderPath();

               // Fail command if cURL is missing
               if (await CheckCurlIsMissingAsync(commands))
               {
                  return false;
               }

               var command = $"curl -sSL {PackageConstants.Debugger.GetVsDbgShUrl} | sh /dev/stdin -u -v {version} -l {debuggerInstallPath}";
               var result = await commands.ExecuteCommandAsync(command);
               outputPaneWriter.Write(result);
            }
         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDebuggerOnlineCompletedFailed, ex.Message);
            return false;
         }

         return true;
      }

      /// <inheritdoc />
      public async Task TryInstallVsDbgOfflineAsync(string version = Constants.Debugger.VersionLatest)
      {
         try
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDebuggerOfflineCommonProgress);
            statusbar?.SetText(Resources.RemoteCommandInstallDebuggerOfflineCommonProgress);

            // Get the CPU architecture to determine which runtime ID to use, ignoring MacOS and Alpine based Linux when determining the needed runtime ID.
            string runtimeId = await GetRuntimeIdAsync();

            // get the download cache folder
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var downloadCachePath = Path.Combine(localAppData, PackageConstants.Debugger.DownloadCacheFolder, runtimeId);

            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDebuggerOfflineOutputPaneProgressDownloading, PackageConstants.Debugger.GetVsDbgPs1Url, version, runtimeId);

            // Download the PS1 script to install the debugger
            using (var httpClient = new HttpClient())
            {
               var response = (await httpClient.GetAsync(new Uri(PackageConstants.Debugger.GetVsDbgPs1Url)))
                  .EnsureSuccessStatusCode();
               var installScript = await response.Content.ReadAsStringAsync();

               using (var psHost = PowerShell.Create())
               {
                  _ = psHost.AddScript(installScript)
                     .AddParameter("Version", version)
                     .AddParameter("RuntimeID", runtimeId)
                     .AddParameter("InstallPath", downloadCachePath);
                  _ = psHost.Invoke();
               }
            }

            var debuggerInstallPath = configurationAggregator.QueryDebuggerInstallFolderPath();

            outputPaneWriter.Write(Resources.RemoteCommandInstallDebuggerOfflineOutputPaneProgressInstalling);

            using (var commandSession = session.CreateCommandSession())
            {
               // remove all files in the target folder, in case the debugger was installed before
              await CleanRemoteFolderAsync(commandSession, debuggerInstallPath);

               // create the directory if it does not yet exist
               await CreateRemoteFolderIfNeededAsync(commandSession, debuggerInstallPath);

               // upload the files
               await bulkCopy.UploadFolderRecursiveAsync(downloadCachePath, debuggerInstallPath);

               // adjust permissions
               _= await commandSession.ExecuteCommandAsync($"chmod +x {debuggerInstallPath}/{PackageConstants.Debugger.BinaryName}");
            }

            // installation completed successfully
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDebuggerOfflineCompletedSuccess);
         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDebuggerOfflineCompletedFailed, ex.Message);
            throw;
         }
      }
      #endregion

      #region Deployment
      /// <inheritdoc />
      public async Task DeployRemoteFolderAsync(string sourcePath, bool clean)
      {
         var targetPath = await tokenReplacer.ReplaceTokensInStringAsync(configurationAggregator.QueryAppFolderPath(), false);

         outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         outputPaneWriter.WriteLine(Resources.RemoteCommandDeployRemoteFolderCommonProgress, sourcePath, targetPath);
         statusbar?.SetText(Resources.RemoteCommandDeployRemoteFolderStatusbarProgress);

         // Clean the remote target if requested
         await CleanFolderAsync(targetPath, clean);

         // copy files using the bulk copy service (SCP or rsync)
         await bulkCopy.UploadFolderRecursiveAsync(sourcePath, targetPath, outputPaneWriter);

         outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         outputPaneWriter.WriteLine(Resources.RemoteCommandDeployRemoteFolderCompletedSuccess);
         statusbar?.SetText(Resources.RemoteCommandDeployRemoteFolderCompletedSuccess);
      }

      /// <inheritdoc />
      public async Task DeployRemoteFolderAsync(string sourcePath, string remoteTargetPath, bool clean)
      {
         outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         outputPaneWriter.WriteLine(Resources.RemoteCommandDeployRemoteFolderCommonProgress, sourcePath, remoteTargetPath);
         statusbar?.SetText(Resources.RemoteCommandDeployRemoteFolderStatusbarProgress);

         // Clean the remote target if requested
         await CleanFolderAsync(remoteTargetPath, clean);

         // copy files using the bulk copy service (SCP or rsync)
         await bulkCopy.UploadFolderRecursiveAsync(sourcePath, remoteTargetPath, outputPaneWriter);

         outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         outputPaneWriter.WriteLine(Resources.RemoteCommandDeployRemoteFolderCompletedSuccess);
         statusbar?.SetText(Resources.RemoteCommandDeployRemoteFolderCompletedSuccess);
      }

      /// <inheritdoc />
      public async Task DeployRemoteFileAsync(string sourceFilePath, string remoteTargetPath)
      {
         outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         outputPaneWriter.WriteLine(Resources.RemoteCommandDeployFileProgress, sourceFilePath, remoteTargetPath);
         statusbar?.SetText(Resources.RemoteCommandDeployFileStatusbarProgress);

         // copy file using the bulk copy service (SCP or rsync)
         await bulkCopy.UploadFileAsync(sourceFilePath, remoteTargetPath);

         outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         outputPaneWriter.WriteLine(Resources.RemoteCommandDeployFileCompletedSuccess, sourceFilePath, remoteTargetPath);
         statusbar?.SetText(Resources.RemoteCommandDeployFileCompletedSuccess);
      }

       /// <inheritdoc />
      public async Task CleanRemoteFolderAsync()
      {
         try
         {
            var targetPath = await tokenReplacer.ReplaceTokensInStringAsync(configurationAggregator.QueryAppFolderPath(), false);

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandCleanRemoteFolderCaption, targetPath);
            statusbar?.SetText(Resources.RemoteCommandCleanRemoteFolderStatusbarProgress);

            await CleanFolderAsync(targetPath, true);

            outputPaneWriter.WriteLine(Resources.RemoteCommandCleanRemoteFolderCompletedSuccess);
         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandCleanRemoteFolderCompletedFailed, ex.Message);
            throw;
         }
      }

      /// <inheritdoc />
      public async Task ChangeRemoteFilePermissionAsync(string remotePath, int permissions)
      {
         try
         {
            var permissionText = $"{permissions}";

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandChangeRemoteFilePermissionCaption, remotePath, permissionText);

            _ = await session.ExecuteSingleCommandAsync(PackageConstants.LinuxShellCommands.FormatChmod(permissionText, remotePath));
         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandChangeRemoteFilePermissionCompletedFailed, ex.Message);
            throw;
         }
      }

      /// <inheritdoc />
      public Task ChangeRemoteFilePermissionAsync(string remotePath, string permissions)
      {
         return ChangeRemoteFilePermissionAsync(remotePath, ConvertPermissions(permissions));
      }

      private static int ConvertPermissions(string permissions)
      {
         int result = 0;
         if (permissions.Length != 9)
         {
            throw new ArgumentException("Permissions string must be 9 characters long.");
         }

         int bitShift = 6;
         for (int block = 0; block < 3; block++)
         {
            int blockResult = 0;
            for(int blockBit = 0; blockBit < 3; blockBit++)
            {
               int position = blockBit + block * 3;
               blockResult += permissions[position] == 'r' ? 4 : 0;
               blockResult += permissions[position] == 'w' ? 2 : 0;
               blockResult += permissions[position] == 'x' ? 1 : 0;
            }
            result += blockResult << bitShift;
            bitShift -= 3;
         }

         return Convert.ToInt32(Convert.ToString(result, 8));
      }

      #endregion

      #region .NET install
      /// <inheritdoc />
      public Task<bool> TryInstallDotNetOnlineAsync(DotnetInstallationKind kind, string channel)
      {
         switch (kind)
         {
            case DotnetInstallationKind.Sdk:
               return TryInstallDotNetSDKOnlineAsync(channel);
            case DotnetInstallationKind.RuntimeNet:
               return TryInstallDotNetRuntimeOnlineAsync(channel, Constants.Dotnet.RuntimeNet);
            case DotnetInstallationKind.RuntimeAspNet:
               return TryInstallDotNetRuntimeOnlineAsync(channel, Constants.Dotnet.RuntimeAspNet);
            default:
               throw new NotSupportedException($"runtime kind '{kind}' not supported");
         }
      }

      /// <inheritdoc />
      public Task TryInstallDotNetOfflineAsync(DotnetInstallationKind kind, string channel)
      {
         switch (kind)
         {
            case DotnetInstallationKind.Sdk:
               return TryInstallDotNetSDKOfflineAsync(channel);
            case DotnetInstallationKind.RuntimeNet:
               return TryInstallDotNetRuntimeOfflineAsync(channel, Constants.Dotnet.RuntimeNet);
            case DotnetInstallationKind.RuntimeAspNet:
               return TryInstallDotNetRuntimeOfflineAsync(channel, Constants.Dotnet.RuntimeAspNet);
            default:
               throw new NotSupportedException($"runtime kind '{kind}' not supported");
         }
      }

      /// <inheritdoc />
      public async Task<string> TryFindDotNetInstallPathAsync()
      {
         logger.LogTrace("TryFindDotNetInstallPathAsync: begin.");

         using (var commandingService = session.CreateCommandSession())
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetOutputPaneProgress);

            // Step 1: try to find 'dotnet' using 'command -v dotnet'
            var commandText = PackageConstants.LinuxShellCommands.FormatCommand(PackageConstants.Dotnet.BinaryName);
            (int statusCode, string result, string error) = await commandingService.TryExecuteCommandAsync(commandText);
            logger.LogInformation("TryFindDotNetInstallPathAsync: command '{CommandText}' returned StatusCode={StatusCode}, result='{Result}', error='{Error}'", commandText, statusCode, result, error);
            if (statusCode == 0)
            {
               // remove the trailing '/dotnet' from the result to get the install folder path
               result = result.Trim('\n').Remove(result.LastIndexOf("/dotnet"));
               outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetOutputPaneFound, result);

               logger.LogTrace("TryFindDotNetInstallPathAsync: end. returns '{Result}'", result);
               return result;
            }

            // Step 2: try to find 'dotnet' using 'bash -lc "command -v dotnet"'
            commandText = PackageConstants.LinuxShellCommands.FormatBashLoginCommand(PackageConstants.Dotnet.BinaryName);
            ( statusCode, result, error) = await commandingService.TryExecuteCommandAsync(commandText);
            logger.LogInformation("TryFindDotNetInstallPathAsync: command '{CommandText}' returned StatusCode={StatusCode}, result='{Result}', error='{Error}'", commandText, statusCode, result, error);
            if (statusCode == 0)
            {
               result = result.Trim('\n').Remove(result.LastIndexOf("/dotnet"));
               outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetOutputPaneFound, result);
            }
            else
            {
               outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetOutputPaneNotFound);
               result = string.Empty;
            }

            logger.LogTrace("TryFindDotNetInstallPathAsync: end. returns '{Result}'", result);
            return result;
         }
      }
      #endregion

      # region HTTPS Setup
      /// <inheritdoc />
      public async Task SetupAspNetDeveloperCertificateAsync(SetupMode mode, byte[] certificate, string password)
      {
         // Path for the 'dotnet' executable
         string dotnetExecutable = UnixPath.Combine(configurationAggregator.QueryDotNetInstallFolderPath(), "dotnet");

         //Check if ASP.NET runtime is installed
         using (var commands = session.CreateCommandSession())
         {
            // Step 1: Check certificate status
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandSetupHttpsCheckProgress);

            (int statusCode, string result, string error) = await commands.TryExecuteCommandAsync($"{dotnetExecutable} dev-certs https --check");

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandSetupHttpsCheckResult, result);

            if (statusCode == 0 && mode == SetupMode.Update)
            {
               // Update mode: exit if a valid cert is present
               outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               outputPaneWriter.WriteLine(Resources.RemoteCommandSetupHttpsCompleted);
               return;
            }

            // Step 2: upload the new certificate
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandSetupHttpsUploadProgress);

            var certificateTargetPath = (await commands.ExecuteCommandAsync("pwd")).Trim('\n');
            certificateTargetPath = UnixPath.Combine(certificateTargetPath, PackageConstants.SecureShell.HttpsCertificateName);

            using (var stream = new MemoryStream(certificate))
            {
               await session.UploadFileAsync(stream, certificateTargetPath);
            }

            // Step 3: import new certificate
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandSetupHttpsInstallProgress);

            (statusCode, result, error) = await commands.TryExecuteCommandAsync($"{dotnetExecutable} dev-certs https --clean --import {certificateTargetPath} --password {password}");
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandSetupHttpsInstallResult, result.Replace('\n', ' '));
            outputPaneWriter.WriteLine(!string.IsNullOrEmpty(error), Resources.RemoteCommandSetupHttpsInstallResult, error.Replace('\n', ' '));

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(statusCode == 0 ? Resources.RemoteCommandSetupHttpsCompleted  : Resources.RemoteCommandSetupHttpsFailed);
         }
      }
      #endregion

      #region Support APIs
      /// <inheritdoc />
      public async Task<string> GetRuntimeIdAsync()
      {
         string runtimeId;
         var cpuArchitecture = (await session.ExecuteSingleCommandAsync("uname -m")).Trim('\n');
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
      #endregion

      #region private methods

      /// <summary>
      /// Creates the remote directory if needed.
      /// </summary>
      /// <param name="commands">The commands service to use.</param>
      /// <param name="remoteTargetPath">The remote target path.</param>
      private static async Task CreateRemoteFolderIfNeededAsync(ISecureShellSessionCommandingService commands, string remoteTargetPath)
      {
         _ = await commands.ExecuteCommandAsync(PackageConstants.LinuxShellCommands.FormatMkDir(remoteTargetPath));
      }

      /// <summary>
      /// Cleans the supplied remote folder.
      /// </summary>
      /// <param name="commands">The commands service to use.</param>
      /// <param name="remoteTargetPath">The remote target path.</param>
      private static async Task CleanRemoteFolderAsync(ISecureShellSessionCommandingService commands, string remoteTargetPath)
      {
         _ = await commands.ExecuteCommandAsync($"[ -d {remoteTargetPath} ] && rm -rf {remoteTargetPath}/*");
      }

      /// <summary>
      /// Cleans the specified folder leaving it empty or creates it if it does not exist.
      /// </summary>
      /// <param name="remoteTargetPath">The absolute path to the remote target path to clean.</param>
      /// <param name="clean">if set to <c>true</c> the folder will be cleaned.</param>
      private async Task CleanFolderAsync(string remoteTargetPath, bool clean)
      {
         ThrowIf.ArgumentNullOrEmpty(remoteTargetPath, nameof(remoteTargetPath));
         using (var commandSession = session.CreateCommandSession())
         {
            if (clean)
            {
               await CleanRemoteFolderAsync(commandSession, remoteTargetPath);
            }

            await CreateRemoteFolderIfNeededAsync(commandSession, remoteTargetPath);
         }
      }

      /// <summary>
      /// Checks if cURL is installed.
      /// </summary>
      /// <param name="commands">The SSH session to use</param>
      /// <returns><c>true</c> if installed; else <c>false</c>.</returns>
      private async Task<bool> CheckCurlIsMissingAsync(ISecureShellSessionCommandingService commands)
      {
         (int exitCode, _, _) = await commands.TryExecuteCommandAsync(PackageConstants.LinuxShellCommands.FormatCommand("curl"));
         if (exitCode != 0)
         {
            outputPaneWriter.WriteLine(Resources.RemoteCommandCommonFailedCurlNotInstalled);
            return true;
         }

         return false;
      }

      /// <summary>
      /// Tries to install the .NET SDK assuming the target device has a direct internet connection.
      /// </summary>
      /// <param name="channel">The channel source holding the version to install.</param>
      /// <returns>A <see cref="Task{Boolean}" />representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      /// <remarks>See Microsoft Docs: https://docs.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual</remarks>
      private async Task<bool> TryInstallDotNetSDKOnlineAsync(string channel)
      {
         try
         {
            using (var commands = session.CreateCommandSession())
            {
               outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetSdkOnlineOutputPaneProgress);

               // Fail command if cURL is missing
               if (await CheckCurlIsMissingAsync(commands))
               {
                  return false;
               }

               var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
               var result = await commands.ExecuteCommandAsync($"curl -sSL {PackageConstants.Dotnet.GetInstallDotnetShUrl} | bash /dev/stdin --channel {channel} --install-dir {dotnetInstallPath}");
               outputPaneWriter.Write(result);
            }
         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetSdkOnlineCompletedFailed, ex.Message);
            return false;
         }

         return true;
      }

      /// <summary>
      /// Tries to install a .NET runtime assuming the target device has a direct internet connection.
      /// </summary>
      /// <param name="channel">The channel source holding the version to install.</param>
      /// <param name="runtime">The runtime  to install.</param>
      /// <returns>A <see cref="Task{Boolean}" />representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      /// <remarks>See Microsoft Docs: https://docs.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual</remarks>
      private async Task<bool> TryInstallDotNetRuntimeOnlineAsync(string channel, string runtime)
      {
         try
         {
            using (var commands = session.CreateCommandSession())
            {
               outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOnlineOutputPaneProgress);

               var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();

               // Fail command if cURL is missing
               if (await CheckCurlIsMissingAsync(commands))
               {
                  return false;
               }

               var result = await commands.ExecuteCommandAsync($"curl -sSL {PackageConstants.Dotnet.GetInstallDotnetShUrl} | bash /dev/stdin --channel {channel} --runtime {runtime} --install-dir {dotnetInstallPath}").ConfigureAwait(true);
               outputPaneWriter.Write(result);
            }

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOnlineCompletedSuccess);
         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOnlineCompletedFailed, ex.Message);
            return false;
         }

         return true;
      }

      /// <summary>
      /// Tries to install the .NET SDK assuming the target device has no internet connection.
      /// </summary>
      /// <param name="channel">The channel source holding the version to install.</param>
      /// <returns>A <see cref="Task"/>representing the asynchronous operation.</returns>
      private async Task TryInstallDotNetSDKOfflineAsync(string channel)
      {
         try
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetSdkOfflineOutputPaneProgress);

            var installerPath = await DownloadDotnetAsync(channel).ConfigureAwait(true);
            await InstallDotnetAsync(installerPath).ConfigureAwait(true);

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetSdkOfflineCompletedSuccess);

         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetSdkOfflineCompletedFailed, ex.Message);
         }
      }

      /// <summary>
      /// Tries to install a .NET runtime assuming the target device has no internet connection.
      /// </summary>
      /// <returns>A <see cref="Task{Boolean}"/>representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      private async Task TryInstallDotNetRuntimeOfflineAsync(string channel, string runtime)
      {
         try
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOfflineOutputPaneProgress);

            var installerPath = await DownloadDotnetAsync(channel, runtime).ConfigureAwait(true);
            await InstallDotnetAsync(installerPath).ConfigureAwait(true);

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOfflineCompletedSuccess);
         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOfflineCompletedFailed, ex.Message);
         }
      }

      private static string BuildDownloadCacheFilePath(string relativePath, string filName)
      {
         var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
         var directoryPath = Path.Combine(localAppData, relativePath);
         var targetPath = Path.Combine(directoryPath, filName);

         if (!Directory.Exists(directoryPath))
         {
            _ = Directory.CreateDirectory(directoryPath);
         }

         return targetPath;
      }

      private async Task<string> DownloadDotnetAsync(string channel, string runtime = null)
      {
         // Get the CPU architecture to determine which runtime ID to use, ignoring MacOS and Alpine based Linux when determining the needed runtime ID.
         string runtimeId = await GetRuntimeIdAsync();

         outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         outputPaneWriter.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneDownloadingScript, PackageConstants.Dotnet.GetInstallDotnetPs1Url, runtimeId);

         // Download the PS1 script to install .NET
         using (var httpClient = new HttpClient())
         {
            string installScript;

            using (var response = await httpClient.GetAsync(new Uri(PackageConstants.Dotnet.GetInstallDotnetPs1Url)))
            {
               _ = response.EnsureSuccessStatusCode();
               installScript = await response.Content.ReadAsStringAsync();
               outputPaneWriter.WriteLine(Resources.RemoteCommandCommonSuccess);
            }

            // Create the runspace so that you can access the pipeline.
            var host = new PSHostCaptureOutput();

            using (var runSpace = RunspaceFactory.CreateRunspace(host))
            {
               runSpace.Open();

               // Create the pipeline.
               using (Pipeline pipe = runSpace.CreatePipeline())
               {
                  var command = new Command(installScript, true);
                  command.Parameters.Add("-Channel", channel);
                  if (!string.IsNullOrEmpty(runtime))
                  {
                     command.Parameters.Add("-Runtime", runtime);
                  }
                  command.Parameters.Add("-Architecture", "x64");
                  command.Parameters.Add("-DryRun");
                  pipe.Commands.Add(command);

                  pipe.Commands.Add("out-default");
                  pipe.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                  _ = pipe.Invoke();
               }
            }

            // parse the download URL from the script output
            if (host.HasError)
            {
               throw new SecureShellSessionException(string.Format(Resources.RemoteCommandInstallDotnetScriptExecutionFailed, host.ErrorText));
            }

            string dotnetDownloadUrl = host.OutputLines.FirstOrDefault(l => l.Contains("URL #0")).Split(' ').LastOrDefault()?.Trim();
            dotnetDownloadUrl = dotnetDownloadUrl.Replace("win-x64", runtimeId).Replace(".zip", ".tar.gz");

            var filePath = BuildDownloadCacheFilePath(PackageConstants.Dotnet.DownloadCacheFolder, Path.GetFileName(dotnetDownloadUrl));
            if (!File.Exists(filePath))
            {
               // download the payload file, store in the cache folder
               outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               outputPaneWriter.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneDownloadingPayload, dotnetDownloadUrl);

               using (var response = await httpClient.GetAsync(new Uri(dotnetDownloadUrl)))
               {
                  _= response.EnsureSuccessStatusCode();

                  using (var stream = File.Create(filePath))
                  {
                     await response.Content.CopyToAsync(stream);
                  }

                  outputPaneWriter.WriteLine(Resources.RemoteCommandCommonSuccess);
               }
            }
            else
            {
               // skip download, use the cache
               outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               outputPaneWriter.WriteLine(Resources.RemoteCommandInstallDotnetOfflineOutputPaneDownloadingSkipped, Path.GetFileName(dotnetDownloadUrl));
            }

            return filePath;
         }
      }

      private async Task InstallDotnetAsync(string filePath)
      {
         using (var commands = session.CreateCommandSession())
         {
            var fileName = Path.GetFileName(filePath);

            var userHome = (await commands.ExecuteCommandAsync(PackageConstants.LinuxShellCommands.Pwd)).Trim('\n');

            var targetPath = UnixPath.Combine(userHome, fileName);
            var installPath = UnixPath.Normalize(configurationAggregator.QueryDotNetInstallFolderPath(), userHome);

            // Install .NET using the following command sequence: "mkdir - p $HOME / dotnet && tar zxf dotnet-sdk - 6.0.201 - linux - x64.tar.gz - C $HOME / dotnet"
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneUploadingPayload, targetPath);

            await session.UploadFileAsync(filePath, targetPath, outputPaneWriter);

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneInstalling, fileName);

            _ = await commands.ExecuteCommandAsync(PackageConstants.LinuxShellCommands.FormatMkDir(installPath));
            _ = await commands.ExecuteCommandAsync($"tar zxf {targetPath} -C {installPath}");
            _ = await commands.ExecuteCommandAsync(PackageConstants.LinuxShellCommands.FormatRmF(targetPath));

            outputPaneWriter.WriteLine(Resources.RemoteCommandCommonSuccess);
         }
      }

      /// <inheritdoc />
      public async Task<Dictionary<string, string>> QueryProcessEnvironmentAsync(string processName)
      {
         var result = new Dictionary<string, string>();

         if (string.IsNullOrWhiteSpace(processName))
         {
            return result;
         }

         try
         {
            logger.LogDebug("QueryProcessEnvironmentAsync: Querying environment from process '{ProcessName}'", processName);

            // Escape the process name to prevent command injection
            // Only allow alphanumeric characters, hyphens, and underscores
            if (!System.Text.RegularExpressions.Regex.IsMatch(processName, @"^[a-zA-Z0-9_-]+$"))
            {
               logger.LogWarning("QueryProcessEnvironmentAsync: Invalid process name '{ProcessName}' - only alphanumeric, hyphens, and underscores are allowed", processName);
               return result;
            }

            // Find the process ID of a process with the given name owned by the current user
            var userName = session.Settings.UserName;
            var findPidCommand = $"pgrep -u {userName} -x \"{processName}\" | head -n 1";
            
            var pidResult = await session.ExecuteSingleCommandAsync(findPidCommand);
            var pid = pidResult.Trim();

            if (string.IsNullOrWhiteSpace(pid) || !int.TryParse(pid, out _))
            {
               logger.LogDebug("QueryProcessEnvironmentAsync: Process '{ProcessName}' not found or not owned by user '{UserName}'", processName, userName);
               return result;
            }

            logger.LogDebug("QueryProcessEnvironmentAsync: Found process '{ProcessName}' with PID {Pid}", processName, pid);

            // Read the environment variables from /proc/{pid}/environ
            var environCommand = $"cat /proc/{pid}/environ";
            var environResult = await session.ExecuteSingleCommandAsync(environCommand);

            // The environ file contains null-terminated strings
            var envVars = environResult.Split('\0', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var envVar in envVars)
            {
               var separatorIndex = envVar.IndexOf('=');
               if (separatorIndex > 0)
               {
                  var key = envVar.Substring(0, separatorIndex);
                  var value = envVar.Substring(separatorIndex + 1);
                  result[key] = value;
               }
            }

            logger.LogDebug("QueryProcessEnvironmentAsync: Successfully retrieved {Count} environment variables from process '{ProcessName}'", result.Count, processName);
         }
         catch (Exception ex)
         {
            logger.LogWarning(ex, "QueryProcessEnvironmentAsync: Failed to query environment from process '{ProcessName}'", processName);
            // Return empty dictionary on error - this is not a critical failure
         }

         return result;
      }
      #endregion
   }
}
