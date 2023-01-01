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
using System.Text;
using System.Threading.Tasks;
using RemoteDebuggerLauncher.PowerShellHost;
using RemoteDebuggerLauncher.Shared;
using Constants = RemoteDebuggerLauncher.Shared.Constants;

namespace RemoteDebuggerLauncher
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
      private readonly ILoggerService logger;
      private readonly IStatusbarService statusbar;

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellRemoteOperations" /> class.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <param name="session">The session to use.</param>
      /// <param name="logger">The logger service instance to use.</param>
      /// <param name="statusbar">Optional statusbar service to report progress.</param>
      internal SecureShellRemoteOperationsService(ConfigurationAggregator configurationAggregator, ISecureShellSessionService session, ILoggerService logger, IStatusbarService statusbar)
      {
         this.configurationAggregator = configurationAggregator;
         this.session = session;
         this.logger = logger;
         this.statusbar = statusbar;
      }

      /// <inheritdoc />
      public bool LogHost { get; set; }

      /// <inheritdoc />
      public async Task CheckConnectionThrowAsync()
      {
         try
         {
            logger.Write(Resources.RemoteCommandCheckConnectionOutputPaneConnectingTo, session.Settings.UserName, session.Settings.HostName);
            statusbar?.SetText(Resources.RemoteCommandCheckConnectionStatusbarProgress, session.Settings.HostName);

            await session.ExecuteSingleCommandAsync("hello echo").ConfigureAwait(true);

            logger.WriteLine(Resources.RemoteCommandCommonSuccess);
            statusbar?.SetText(Resources.RemoteCommandCheckConnectionStatusbarCompletedSuccess, session.Settings.HostName);
         }
         catch (Exception ex)
         {
            // whatever exception is thrown indicates a problem
            logger.WriteLine(Resources.RemoteCommandCommonFailed, ex.Message);
            throw new RemoteDebuggerLauncherException($"Cannot connect to {session.Settings.UserName}@{session.Settings.HostName} : {ex.Message}");
         }
      }

      /// <inheritdoc />
      public async Task<string> QueryUserHomeDirectoryAsync()
      {
         try
         {
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.Write("Query User Home: ");
            var result = (await session.ExecuteSingleCommandAsync("pwd").ConfigureAwait(true)).Trim('\n');
            logger.WriteLine(result);
            return result;
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteLine(Resources.RemoteCommandCommonFailed, ex.Message);
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
               logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               logger.WriteLine(Resources.RemoteCommandInstallDebuggerOnlineCommonProgress);
               statusbar?.SetText(Resources.RemoteCommandInstallDebuggerOnlineCommonProgress);

               var debuggerInstallPath = configurationAggregator.QueryDebuggerInstallFolderPath();
               var command = $"curl -sSL {PackageConstants.Debugger.GetVsDbgShUrl} | sh /dev/stdin -u -v {version} -l {debuggerInstallPath}";
               var result = await commands.ExecuteCommandAsync(command).ConfigureAwait(true);
               logger.Write(result);
            }
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteLine(Resources.RemoteCommandInstallDebuggerOnlineCompletedFailed, ex.Message);
            return false;
         }

         return true;
      }

      /// <inheritdoc />
      public async Task TryInstallVsDbgOfflineAsync(string version = Constants.Debugger.VersionLatest)
      {
         try
         {
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDebuggerOfflineCommonProgress);
            statusbar?.SetText(Resources.RemoteCommandInstallDebuggerOfflineCommonProgress);

            // Get the CPU architecture to determine which runtime ID to use, ignoring MacOS and Alpine based Linux when determining the needed runtime ID.
            string runtimeId = await GetRuntimeIdAsync().ConfigureAwait(true);

            // get the download cache folder
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var downloadCachePath = Path.Combine(localAppData, PackageConstants.Debugger.DownloadCacheFolder, runtimeId);

            logger.WriteLine(Resources.RemoteCommandInstallDebuggerOfflineOutputPaneProgressDownloading, PackageConstants.Debugger.GetVsDbgPs1Url, version, runtimeId);

            // Download the PS1 script to install the debugger
            using (var httpClient = new HttpClient())
            {
               var response = await httpClient.GetAsync(new Uri(PackageConstants.Debugger.GetVsDbgPs1Url)).ConfigureAwait(true);
               response.EnsureSuccessStatusCode();
               var installScript = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

               using (var psHost = PowerShell.Create())
               {
                  psHost.AddScript(installScript)
                     .AddParameter("Version", version)
                     .AddParameter("RuntimeID", runtimeId)
                     .AddParameter("InstallPath", downloadCachePath);
                  var result = psHost.Invoke();
               }
            }

            var debuggerInstallPath = configurationAggregator.QueryDebuggerInstallFolderPath();

            logger.Write(Resources.RemoteCommandInstallDebuggerOfflineOutputPaneProgressInstalling);

            using (var commandSession = session.CreateCommandSession())
            {
               // remove all files in the target folder, in case the debugger was installed before
               await commandSession.ExecuteCommandAsync($"[ -d {debuggerInstallPath} ] | rm -rf {debuggerInstallPath}/*").ConfigureAwait(true);

               // create the directory if it does not jet exist
               await commandSession.ExecuteCommandAsync($"mkdir -p {debuggerInstallPath}").ConfigureAwait(true);

               // upload the files
               await session.UploadFolderRecursiveAsync(downloadCachePath, debuggerInstallPath).ConfigureAwait(true);

               // adjust permissions
               await commandSession.ExecuteCommandAsync($"chmod +x {debuggerInstallPath}/{PackageConstants.Debugger.BinaryName}").ConfigureAwait(true);
            }

            // installation completed successfully
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDebuggerOfflineCompletedSuccess);
         }
         catch (SecureShellSessionException ex)
         {
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDebuggerOfflineCompletedFailed, ex.Message);
            throw;
         }
      }
      #endregion

      #region Deployment
      /// <inheritdoc />
      public async Task DeployRemoteFolderAsync(string sourcePath, bool clean)
      {
         var targetPath = configurationAggregator.QueryAppFolderPath();

         logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         logger.WriteLine(Resources.RemoteCommandDeployRemoveFolderOutputPaneProgress, sourcePath, targetPath);
         statusbar?.SetText(Resources.RemoteCommandDeployRemoveFolderStatusbarProgress);

         // Clean the remote target if requested
         if (clean)
         {
            using (var commandSession = session.CreateCommandSession())
            {
               await commandSession.ExecuteCommandAsync($"[ -d {targetPath} ] | rm -rf {targetPath}/*").ConfigureAwait(true);
               await commandSession.ExecuteCommandAsync($"mkdir -p {targetPath}").ConfigureAwait(true);
            }
         }

         // copy files using SCP
         await session.UploadFolderRecursiveAsync(sourcePath, targetPath, logger).ConfigureAwait(true);

         logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         logger.WriteLine(Resources.RemoteCommandDeployRemoveFolderCompletedSuccess);
         statusbar?.SetText(Resources.RemoteCommandDeployRemoveFolderCompletedSuccess);
      }

       /// <inheritdoc />
      public async Task CleanRemoteFolderAsync()
      {
         try
         {
            var targetPath = configurationAggregator.QueryAppFolderPath();

            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandCleanRemoteFolderCaption, targetPath);
            statusbar?.SetText(Resources.RemoteCommandCleanRemoteFolderStatusbarProgress);

            using (var commandSession = session.CreateCommandSession())
            {
               await commandSession.ExecuteCommandAsync($"[ -d {targetPath} ] | rm -rf {targetPath}/*").ConfigureAwait(true);
               await commandSession.ExecuteCommandAsync($"mkdir -p {targetPath}").ConfigureAwait(true);
            }

            logger.WriteLine(Resources.RemoteCommandCleanRemoteFolderCompletedSuccess);
         }
         catch (SecureShellSessionException ex)
         {
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandCleanRemoteFolderCompletedFailed, ex.Message);
            throw;
         }
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
               logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               logger.WriteLine(Resources.RemoteCommandInstallDotnetSdkOnlineOutputPaneProgress);

               var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
               var result = await commands.ExecuteCommandAsync($"curl -sSL {PackageConstants.Dotnet.GetInstallDotnetShUrl} | bash /dev/stdin --channel {channel} --install-dir {dotnetInstallPath}").ConfigureAwait(true);
               logger.Write(result);
            }
         }
         catch (SecureShellSessionException ex)
         {
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDotnetSdkOnlineCompletedFailed, ex.Message);
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
               logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               logger.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOnlineOutputPaneProgress);

               var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
               var result = await commands.ExecuteCommandAsync($"curl -sSL {PackageConstants.Dotnet.GetInstallDotnetShUrl} | bash /dev/stdin --channel {channel} --runtime {runtime} --install-dir {dotnetInstallPath}").ConfigureAwait(true);
               logger.Write(result);
            }

            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOnlineCompletedSuccess);
         }
         catch (SecureShellSessionException ex)
         {
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOnlineCompletedFailed, ex.Message);
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
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDotnetSdkOfflineOutputPaneProgress);

            var installerPath = await DownloadDotnetAsync(channel).ConfigureAwait(true);
            await InstallDotnetAsync(installerPath).ConfigureAwait(true);

            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDotnetSdkOfflineCompletedSuccess);

         }
         catch (SecureShellSessionException ex)
         {
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDotnetSdkOfflineCompletedFailed, ex.Message);
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
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOfflineOutputPaneProgress);

            var installerPath = await DownloadDotnetAsync(channel, runtime).ConfigureAwait(true);
            await InstallDotnetAsync(installerPath).ConfigureAwait(true);

            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOfflineCompletedSuccess);
         }
         catch (SecureShellSessionException ex)
         {
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.WriteLine(Resources.RemoteCommandInstallDotnetRuntimeOfflineCompletedFailed, ex.Message);
         }
      }
      #endregion

      private async Task<string> GetRuntimeIdAsync()
      {
         string runtimeId;
         var cpuArchitecture = (await session.ExecuteSingleCommandAsync("uname -m").ConfigureAwait(true)).Trim('\n');
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

      private static string BuildDownloadCacheFilePath(string relativePath, string filName)
      {
         var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
         var directoryPath = Path.Combine(localAppData, relativePath);
         var targetPath = Path.Combine(directoryPath, filName);

         if (!Directory.Exists(directoryPath))
         {
            Directory.CreateDirectory(directoryPath);
         }

         return targetPath;
      }

      private async Task<string> DownloadDotnetAsync(string channel, string runtime = null)
      {
         // Get the CPU architecture to determine which runtime ID to use, ignoring MacOS and Alpine based Linux when determining the needed runtime ID.
         string runtimeId = await GetRuntimeIdAsync().ConfigureAwait(true);

         var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
         string installScript;
         string dotnetDownloadUrl;

         logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         logger.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneDownloadingScript, PackageConstants.Dotnet.GetInstallDotnetPs1Url, runtimeId);

         // Download the PS1 script to install .NET
         using (var httpClient = new HttpClient())
         {

            using (var response = await httpClient.GetAsync(new Uri(PackageConstants.Dotnet.GetInstallDotnetPs1Url)).ConfigureAwait(true))
            {
               response.EnsureSuccessStatusCode();
               installScript = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
               logger.WriteLine(Resources.RemoteCommandCommonSuccess);
            }

            // Create the runspace so that you can access the pipeline.
            var host = new CaptureOutputPSHost();
             
            using (var runSpace = RunspaceFactory.CreateRunspace(host))
            {
               runSpace.Open();

               // Create the pipeline.
               using (Pipeline pipe = runSpace.CreatePipeline())
               {
                  var command = new Command(installScript, true);
                  command.Parameters.Add("-Channel", channel);
                  if (!String.IsNullOrEmpty(runtime))
                  {
                     command.Parameters.Add("-Runtime", runtime);
                  }
                  command.Parameters.Add("-Architecture", "x64");
                  command.Parameters.Add("-DryRun");
                  pipe.Commands.Add(command);

                  pipe.Commands.Add("out-default");
                  pipe.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                  pipe.Invoke();
               }
            }

            // parse the download URL from the script output
            if (host.HasError)
            {
               throw new SecureShellSessionException(String.Format(Resources.RemoteCommandInstallDotnetScriptExecutionFailed, host.ErrorText));
            }

            dotnetDownloadUrl = host.OutputLines.FirstOrDefault(l => l.Contains("URL #0")).Split(' ').LastOrDefault()?.Trim();
            dotnetDownloadUrl = dotnetDownloadUrl.Replace("win-x64", runtimeId).Replace(".zip", ".tar.gz");

            var filePath = BuildDownloadCacheFilePath(PackageConstants.Dotnet.DownloadCacheFolder, Path.GetFileName(dotnetDownloadUrl));
            if (!File.Exists(filePath))
            {
               // download the payload file, store in the cache folder
               logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               logger.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneDownloadingPayload, dotnetDownloadUrl);

               using (var response = await httpClient.GetAsync(new Uri(dotnetDownloadUrl)).ConfigureAwait(true))
               {
                  response.EnsureSuccessStatusCode();

                  using (var stream = File.Create(filePath))
                  {
                     await response.Content.CopyToAsync(stream).ConfigureAwait(true);
                  }

                  logger.WriteLine(Resources.RemoteCommandCommonSuccess);
               }
            }
            else
            {
               // skip download, use the cache
               logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
               logger.WriteLine(Resources.RemoteCommandInstallDotnetOfflineOutputPaneDownloadingSkipped, Path.GetFileName(dotnetDownloadUrl));
            }

            return filePath;
         }
      }

      private async Task InstallDotnetAsync(string filePath)
      {
         using (var commands = session.CreateCommandSession())
         {
            var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
            var fileName = Path.GetFileName(filePath);

            var userHome = (await commands.ExecuteCommandAsync("pwd").ConfigureAwait(true)).Trim('\n');

            var targetPath = UnixPath.Combine(userHome, fileName);
            var installPath = UnixPath.Normalize(configurationAggregator.QueryDotNetInstallFolderPath(), userHome);

            //"mkdir - p $HOME / dotnet && tar zxf dotnet-sdk - 6.0.201 - linux - x64.tar.gz - C $HOME / dotnet";
            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneUploadingPayload, targetPath);

            await session.UploadFileAsync(filePath, targetPath, logger).ConfigureAwait(true);

            logger.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            logger.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneInstalling, fileName);

            var response = await commands.ExecuteCommandAsync($"mkdir -p {installPath}").ConfigureAwait(true);
            await commands.ExecuteCommandAsync($"tar zxf {targetPath} -C {installPath}").ConfigureAwait(true);
            await commands.ExecuteCommandAsync($"rm -f {targetPath}").ConfigureAwait(true);

            logger.WriteLine(Resources.RemoteCommandCommonSuccess);
         }
      }
   }
}
