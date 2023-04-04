// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Http;
using System.Threading.Tasks;
using RemoteDebuggerLauncher.PowerShellHost;
using RemoteDebuggerLauncher.Shared;
using Constants = RemoteDebuggerLauncher.Shared.Constants;

namespace RemoteDebuggerLauncher.SecureShell
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
      private readonly IOutputPaneWriterService outputPaneWriter;
      private readonly IStatusbarService statusbar;

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellRemoteOperations" /> class.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <param name="session">The session to use.</param>
      /// <param name="outputPaneWriter">The output pane writer service instance to use.</param>
      /// <param name="statusbar">Optional statusbar service to report progress.</param>
      internal SecureShellRemoteOperationsService(ConfigurationAggregator configurationAggregator, ISecureShellSessionService session, IOutputPaneWriterService outputPaneWriter, IStatusbarService statusbar)
      {
         this.configurationAggregator = configurationAggregator;
         this.session = session;
         this.outputPaneWriter = outputPaneWriter;
         this.statusbar = statusbar;
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
               _= await commandSession.ExecuteCommandAsync($"[ -d {debuggerInstallPath} ] | rm -rf {debuggerInstallPath}/*");

               // create the directory if it does not jet exist
               _ = await commandSession.ExecuteCommandAsync($"mkdir -p {debuggerInstallPath}");

               // upload the files
               await session.UploadFolderRecursiveAsync(downloadCachePath, debuggerInstallPath);

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
         var targetPath = configurationAggregator.QueryAppFolderPath();

         outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         outputPaneWriter.WriteLine(Resources.RemoteCommandDeployRemoteFolderOutputPaneProgress, sourcePath, targetPath);
         statusbar?.SetText(Resources.RemoteCommandDeployRemoteFolderStatusbarProgress);

         // Clean the remote target if requested
         if (clean)
         {
            using (var commandSession = session.CreateCommandSession())
            {
               _ = await commandSession.ExecuteCommandAsync($"[ -d {targetPath} ] | rm -rf {targetPath}/*");
               _ = await commandSession.ExecuteCommandAsync($"mkdir -p {targetPath}");
            }
         }

         // copy files using SCP
         await session.UploadFolderRecursiveAsync(sourcePath, targetPath, outputPaneWriter).ConfigureAwait(true);

         outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
         outputPaneWriter.WriteLine(Resources.RemoteCommandDeployRemoteFolderCompletedSuccess);
         statusbar?.SetText(Resources.RemoteCommandDeployRemoteFolderCompletedSuccess);
      }

       /// <inheritdoc />
      public async Task CleanRemoteFolderAsync()
      {
         try
         {
            var targetPath = configurationAggregator.QueryAppFolderPath();

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandCleanRemoteFolderCaption, targetPath);
            statusbar?.SetText(Resources.RemoteCommandCleanRemoteFolderStatusbarProgress);

            using (var commandSession = session.CreateCommandSession())
            {
               _ = await commandSession.ExecuteCommandAsync($"[ -d {targetPath} ] | rm -rf {targetPath}/*").ConfigureAwait(true);
               _ = await commandSession.ExecuteCommandAsync($"mkdir -p {targetPath}").ConfigureAwait(true);
            }

            outputPaneWriter.WriteLine(Resources.RemoteCommandCleanRemoteFolderCompletedSuccess);
         }
         catch (SecureShellSessionException ex)
         {
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.WriteLine(Resources.RemoteCommandCleanRemoteFolderCompletedFailed, ex.Message);
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
      #endregion

      private async Task<bool> CheckCurlIsMissingAsync(ISecureShellSessionCommandingService commands)
      {
         (int exitCode, _) = await commands.TryExecuteCommandAsync("command -v curl");
         if (exitCode != 0)
         {
            outputPaneWriter.WriteLine(Resources.RemoteCommandCommonFailedCurlNotInstalled);
            return true;
         }

         return false;
      }

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

            var userHome = (await commands.ExecuteCommandAsync("pwd")).Trim('\n');

            var targetPath = UnixPath.Combine(userHome, fileName);
            var installPath = UnixPath.Normalize(configurationAggregator.QueryDotNetInstallFolderPath(), userHome);

            // Install .NET using the following command sequence: "mkdir - p $HOME / dotnet && tar zxf dotnet-sdk - 6.0.201 - linux - x64.tar.gz - C $HOME / dotnet"
            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneUploadingPayload, targetPath);

            await session.UploadFileAsync(filePath, targetPath, outputPaneWriter);

            outputPaneWriter.Write(LogHost, Resources.RemoteCommandCommonSshTarget, session.Settings.UserName, session.Settings.HostName);
            outputPaneWriter.Write(Resources.RemoteCommandInstallDotnetOfflineOutputPaneInstalling, fileName);

            _ = await commands.ExecuteCommandAsync($"mkdir -p {installPath}");
            _ = await commands.ExecuteCommandAsync($"tar zxf {targetPath} -C {installPath}");
            _ = await commands.ExecuteCommandAsync($"rm -f {targetPath}");

            outputPaneWriter.WriteLine(Resources.RemoteCommandCommonSuccess);
         }
      }
   }
}
