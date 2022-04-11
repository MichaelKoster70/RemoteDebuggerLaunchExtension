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
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Holds the high level operations performed on the remote device.
   /// </summary>
   internal class SecureShellRemoteOperations
   {
      private readonly ConfigurationAggregator configurationAggregator;
      private readonly SecureShellSession session;
      private readonly ILoggerService logger;

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellRemoteOperations" /> class.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <param name="session">The session to use.</param>
      /// <param name="logger">The logger service instance to use.</param>
      internal SecureShellRemoteOperations(ConfigurationAggregator configurationAggregator, SecureShellSession session, ILoggerService logger)
      {
         this.configurationAggregator = configurationAggregator;
         this.session = session;
         this.logger = logger;
      }

      /// <summary>
      /// Creates a <see cref="SecureShellRemoteOperations" /> with settings read from the supplied configuration.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator to read the settings from.</param>
      /// <param name="logger">The logger instance to use.</param>
      /// <returns>A <see cref="SecureShellRemoteOperations" /> instance.</returns>
      /// <exception cref="ArgumentNullException">configurationAggregator is null.</exception>
      public static SecureShellRemoteOperations Create(ConfigurationAggregator configurationAggregator, ILoggerService logger)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));

         var session = SecureShellSession.Create(configurationAggregator);
         return new SecureShellRemoteOperations(configurationAggregator, session, logger);
      }

      /// <summary>
      /// Gets or sets a value indicating whether the Host name should be logged to the output pane.
      /// </summary>
      /// <value><c>true</c> to append to log; otherwise, <c>false</c>.</value>
      public bool LogHost { get; set; }

      /// <summary>
      /// Checks whether a connection with the remove device can be established.
      /// </summary>
      /// <returns>A Task representing the asynchronous operation.</returns>
      /// <exception cref="RemoteDebuggerLauncherException">Thrown when connection cannot be established.</exception>
      public async Task CheckConnectionThrowAsync()
      {
         try
         {
            logger.WriteLineOutputExtensionPane("--------------------------------------------------");
            logger.WriteOutputExtensionPane($"Connecting to {session.Settings.UserName}@{ session.Settings.HostName}... ");
            await session.ExecuteSingleCommandAsync("hello echo").ConfigureAwait(true);
            logger.WriteLineOutputExtensionPane("OK");
         }
         catch (Exception ex)
         {
            // whatever exception is thrown indicates a problem
            logger.WriteLineOutputExtensionPane($"FAILED: {ex.Message}");
            throw new RemoteDebuggerLauncherException($"Cannot connect to {session.Settings.UserName}@{ session.Settings.HostName} : {ex.Message}");
         }
      }

      public async Task<string> QueryUserHomeDirectoryAsync()
      {
         try
         {
            logger.WriteLineOutputExtensionPane("--------------------------------------------------");
            logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}]");
            logger.WriteOutputExtensionPane("Query User Home: ");
            var result = (await session.ExecuteSingleCommandAsync("pwd").ConfigureAwait(true)).Trim('\n');
            logger.WriteLineOutputExtensionPane(result);
            return result;
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteOutputExtensionPane($"FAILED: {ex.Message})\r\n");
            return string.Empty;
         }
      }

      #region VS Code Debugger
      /// <summary>
      /// Tries to install the VS Code assuming the target device has a direct internet connection.
      /// Removes the previous installation if present.
      /// </summary>
      /// <param name="version">The version to install.</param>
      /// <returns>A <see cref="Task{Boolean}" />representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      public async Task<bool> TryInstallVsDbgOnlineAsync(string version = Constants.Debugger.VersionLatest)
      {
         try
         {
            using (var commands = session.CreateCommandSession())
            {
               logger.WriteLineOutputExtensionPane("--------------------------------------------------");
               logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}] ");
               logger.WriteLineOutputExtensionPane("Installing VS Code Debugger Online");

               var debuggerInstallPath = configurationAggregator.QueryDebuggerInstallFolderPath();
               var command = $"curl -sSL {PackageConstants.Debugger.GetVsDbgShUrl} | sh /dev/stdin -u -v {version} -l {debuggerInstallPath}";
               var result = await commands.ExecuteCommandAsync(command);
               logger.WriteOutputExtensionPane(result);
            }
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteOutputExtensionPane($"FAILED to install debugger: {ex.Message})\r\n");
            return false;
         }

         return true;
      }

      /// <summary>
      /// Tries to install the VS Code assuming the target device has no internet connection.
      /// Removes the previous installation if present.
      /// </summary>
      /// <returns>A <see cref="Task"/>representing the asynchronous operation.</returns>
      /// <remarks>
      /// The downloaded vsdbg version gets cached under %localappdata%\RemoteDebuggerLauncher\vsdbg\vs2022
      /// </remarks>
      public async Task TryInstallVsDbgOfflineAsync(string version = Constants.Debugger.VersionLatest)
      {
         try
         {
            logger.WriteLineOutputExtensionPane("--------------------------------------------------");
            logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}] ");
            logger.WriteLineOutputExtensionPane("Installing VS Code Debugger Offline");

            // Get the CPU architecture to determine which runtime ID to use, ignoring MacOS and Alpine based Linux when determining the needed runtime ID.
            string runtimeId = await GetRuntimeIdAsync();

            // get the download cache folder
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var downloadCachePath = Path.Combine(localAppData, PackageConstants.Debugger.DownloadCacheFolder, runtimeId);

            logger.WriteOutputExtensionPane($"Downloading URL:{PackageConstants.Debugger.GetVsDbgPs1Url}, Version: {version}, RuntimeID:{runtimeId}\r\n");

            // Download the PS1 script to install the debugger
            using (var httpClient = new HttpClient())
            {
               var response = await httpClient.GetAsync(PackageConstants.Debugger.GetVsDbgPs1Url);
               response.EnsureSuccessStatusCode();
               var installScript = await response.Content.ReadAsStringAsync();

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

            logger.WriteOutputExtensionPane("$Installing ");

            using (var commandSession = session.CreateCommandSession())
            {
               // remove all files in the target folder, in case the debugger was installed before
               await commandSession.ExecuteCommandAsync($"[ -d {debuggerInstallPath} ] | rm -rf {debuggerInstallPath}/*");

               // create the directory if it does not jet exist
               await commandSession.ExecuteCommandAsync($"mkdir -p {debuggerInstallPath}");

               // upload the files
               await session.UploadFolderRecursiveAsync(downloadCachePath, debuggerInstallPath);

               // adjust permissions
               await commandSession.ExecuteCommandAsync($"chmod +x {debuggerInstallPath}/{PackageConstants.Debugger.BinaryName}");
            }

            logger.WriteLineOutputExtensionPane("DONE");
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteLineOutputExtensionPane($"FAILED to install debugger: {ex.Message}");
            throw;
         }
      }
      #endregion

      public async Task DeployRemoteFolderAsync(string sourcePath, bool clean)
      {
         var targetPath = configurationAggregator.QueryAppFolderPath();

         logger.WriteLineOutputExtensionPane("--------------------------------------------------");
         logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}] ");
         logger.WriteLineOutputExtensionPane($"Deploying source: {sourcePath}, target: {targetPath}");

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
         await session.UploadFolderRecursiveAsync(sourcePath, targetPath);
      }

      public async Task CleanRemoteFolderAsync()
      {
         try
         {
            var targetPath = configurationAggregator.QueryAppFolderPath();

            logger.WriteLineOutputExtensionPane("--------------------------------------------------");
            logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}] ");
            logger.WriteLineOutputExtensionPane($"Cleaning target: {targetPath}");

            using (var commandSession = session.CreateCommandSession())
            {
               await commandSession.ExecuteCommandAsync($"[ -d {targetPath} ] | rm -rf {targetPath}/*").ConfigureAwait(true);
               await commandSession.ExecuteCommandAsync($"mkdir -p {targetPath}").ConfigureAwait(true);
            }
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteLineOutputExtensionPane($"FAILED: {ex.Message}");
            throw;
         }
      }

      #region .NET install
      /// <summary>
      /// Tries to install .NET assuming the target device has a direct internet connection.
      /// </summary>
      /// <param name="kind">The kind of installation to perform.</param>
      /// <param name="channel">The channel source holding the version to install.</param>
      /// <returns>A <see cref="Task{Boolean}" />representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      /// <exception cref="System.NotSupportedException">runtime kind not supported.</exception>
      /// <remarks>See Microsoft Docs: https://docs.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual</remarks>
      public Task<bool> TryInstallDotNetOnlineAsync(DotnetInstallationKind kind, string channel)
      {
         switch(kind)
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

      /// <summary>
      /// Tries to install .NET assuming the target device has no internet connection.
      /// </summary>
      /// <param name="kind">The kind of installation to perform.</param>
      /// <param name="channel">The channel source holding the version to install.</param>
      /// <returns>A <see cref="Task"/>representing the asynchronous operation.</returns>
      /// <exception cref="System.NotSupportedException">runtime kind not supported.</exception>
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
               logger.WriteLineOutputExtensionPane("--------------------------------------------------");
               logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}] ");
               logger.WriteLineOutputExtensionPane("Installing .NET SDK Online");

               var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
               var result = await commands.ExecuteCommandAsync($"curl -sSL {PackageConstants.Dotnet.GetInstallDotnetShUrl} | bash /dev/stdin --channel {channel} --install-dir {dotnetInstallPath}");
               logger.WriteOutputExtensionPane(result);
            }
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteOutputExtensionPane($"FAILED to install .NET SDK: {ex.Message})\r\n");
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
      private async Task<bool> TryInstallDotNetRuntimeOnlineAsync(string channel, string runtime)
      {
         try
         {
            using (var commands = session.CreateCommandSession())
            {
               logger.WriteLineOutputExtensionPane("--------------------------------------------------");
               logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}] ");
               logger.WriteLineOutputExtensionPane("Installing .NET runtime Online\r\n");

               var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
               var result = await commands.ExecuteCommandAsync($"curl -sSL {PackageConstants.Dotnet.GetInstallDotnetShUrl} | bash /dev/stdin --channel {channel} --runtime {runtime} --install-dir {dotnetInstallPath}");
               logger.WriteOutputExtensionPane(result);
            }
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteOutputExtensionPane($"FAILED to install .NET runtime: {ex.Message})\r\n");
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
            logger.WriteLineOutputExtensionPane("--------------------------------------------------");
            logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}] ");
            logger.WriteLineOutputExtensionPane("Installing .NET SDK Offline");

            var installerPath = await DownloadDotnetAsync(channel);
            await InstallDotnetAsync(installerPath);

            logger.WriteOutputExtensionPane("OK");
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteLineOutputExtensionPane($"FAILED to install .NET SDK: {ex.Message}");
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
            logger.WriteLineOutputExtensionPane("--------------------------------------------------");
            logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}] ");
            logger.WriteLineOutputExtensionPane("Installing .NET runtime Offline");

            var installerPath = await DownloadDotnetAsync(channel, runtime);
            await InstallDotnetAsync(installerPath);

            logger.WriteOutputExtensionPane("OK");
         }
         catch (SecureShellSessionException ex)
         {
            logger.WriteLineOutputExtensionPane($"FAILED to install .NET runtime: {ex.Message}");
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
         string runtimeId = await GetRuntimeIdAsync();

         var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
         string installScript;
         string dotnetDownloadUrl;

         logger.WriteLineOutputExtensionPane($"Downloading script URL: {PackageConstants.Dotnet.GetInstallDotnetPs1Url}, RuntimeID: {runtimeId}");

         // Download the PS1 script to install .NET
         using (var httpClient = new HttpClient())
         {
            using (var response = await httpClient.GetAsync(PackageConstants.Dotnet.GetInstallDotnetPs1Url))
            {
               response.EnsureSuccessStatusCode();
               installScript = await response.Content.ReadAsStringAsync();
            }

            // Create the runspace so that you can access the pipeline.
            CustomPSHost host = new CustomPSHost();

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
               throw new SecureShellSessionException($"Script execution failed with '{host.ErrorText}'");
            }

            dotnetDownloadUrl = host.OutputLines.FirstOrDefault(l => l.Contains("URL #0 - primary")).Split(' ').LastOrDefault()?.Trim();
            dotnetDownloadUrl = dotnetDownloadUrl.Replace("win-x64", runtimeId).Replace(".zip", ".tar.gz");
            logger.WriteLineOutputExtensionPane($"Downloading {dotnetDownloadUrl} ");

            var filePath = BuildDownloadCacheFilePath(PackageConstants.Dotnet.DownloadCacheFolder, Path.GetFileName(dotnetDownloadUrl));

            // download the payload file, store in the cache folder
            using (var response = await httpClient.GetAsync(dotnetDownloadUrl))
            {
               response.EnsureSuccessStatusCode();

               using (var stream = File.Create(filePath))
               {
                  await response.Content.CopyToAsync(stream);
               }
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

            //"mkdir - p $HOME / dotnet && tar zxf dotnet-sdk - 6.0.201 - linux - x64.tar.gz - C $HOME / dotnet";
            await session.UploadFileAsync(filePath, targetPath);
         }
      }
   }
}
