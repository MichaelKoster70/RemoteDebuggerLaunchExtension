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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

      /// <summary>
      /// Tries to install the VS Code assuming the target device has a direct internet connection.
      /// </summary>
      /// <returns>A <see cref="Task{Boolean}"/>representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      public async Task<bool> TryInstallVsDbgOnlineAsync()
      {
         try
         {
            using (var commands = session.CreateCommandSession())
            {
               logger.WriteLineOutputExtensionPane("--------------------------------------------------");
               logger.WriteLineOutputExtensionPane("Installing VS Code Debugger Online\r\n");

               var debuggerInstallPath = configurationAggregator.QueryDebuggerInstallFolderPath();
               var result = await commands.ExecuteCommandAsync($"curl -sSL {PackageConstants.Debugger.GetVsDbgShUrl} | sh /dev/stdin -v {PackageConstants.Debugger.Version} -l {debuggerInstallPath}");
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
      /// </summary>
      /// <returns>A <see cref="Task"/>representing the asynchronous operation.</returns>
      /// <remarks>
      /// The downloaded vsdbg version gets cached under %localappdata%\RemoteDebuggerLauncher\vsdbg\vs2022
      /// </remarks>
      public async Task TryInstallVsDbgOfflineAsync()
      {
         try
         {
            logger.WriteLineOutputExtensionPane("--------------------------------------------------");
            logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}]");
            logger.WriteLineOutputExtensionPane("Installing VS Code Debugger Offline");

            // Get the CPU architecture to determine which runtime ID to use, ignoring MacOS and Alpine based Linux when determining the needed runtime ID.
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
                  throw new RemoteDebuggerLauncherException("Cannot install VS Code Debugger: unknown CPU architecture");
            }

            // get the download cache folder
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var downloadCachePath = Path.Combine(localAppData, PackageConstants.Debugger.DownloadCacheFolder, runtimeId);

            logger.WriteOutputExtensionPane($"Downloading URL:{PackageConstants.Debugger.GetVsDbgPs1Url}, Version: {PackageConstants.Debugger.Version}, RuntimeID:{runtimeId}\r\n");

            // Download the PS1 script to install the debugger
            using (var httpClient = new HttpClient())
            {
               var response = await httpClient.GetAsync(PackageConstants.Debugger.GetVsDbgPs1Url);
               response.EnsureSuccessStatusCode();
               var installScript = await response.Content.ReadAsStringAsync();

               using (var psHost = PowerShell.Create())
               {
                  psHost.AddScript(installScript)
                     .AddParameter("Version", PackageConstants.Debugger.Version)
                     .AddParameter("RuntimeID", runtimeId)
                     .AddParameter("InstallPath", downloadCachePath);
                  var result = psHost.Invoke();
               }
            }

            var debuggerInstallPath = configurationAggregator.QueryDebuggerInstallFolderPath();

            logger.WriteOutputExtensionPane("$Installing ");

            using (var commandSession = session.CreateCommandSession())
            {
               // remove all files in the target folder, in case the
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

      public async Task DeployRemoteFolderAsync(string sourcePath, bool clean)
      {
         var targetPath = configurationAggregator.QueryAppFolderPath();

         logger.WriteLineOutputExtensionPane("--------------------------------------------------");
         logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}]");
         logger.WriteLineOutputExtensionPane($"Deploying source: {sourcePath}, sarget: {targetPath}");

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
            logger.WriteOutputExtensionPane(LogHost, $"[SSH {session.Settings.UserName}@{ session.Settings.HostName}]");
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

      public Task<bool> TryInstallDotNetSDKOnlineAsync()
      {
         return TryInstallDotNetSDKOnlineAsync("LTS");
      }

      /// <summary>
      /// Tries to install the .NET SDK assuming the target device has a direct internet connection.
      /// </summary>
      /// <returns>A <see cref="Task{Boolean}"/>representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      public async Task<bool> TryInstallDotNetSDKOnlineAsync(string channel)
      {
         try
         {
            using (var commands = session.CreateCommandSession())
            {
               logger.WriteLineOutputExtensionPane("--------------------------------------------------");
               logger.WriteLineOutputExtensionPane("Installing .NET SDK Online\r\n");

               var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
               var result = await commands.ExecuteCommandAsync($"curl -sSL {PackageConstants.Dotnet.GetInstallDotnetShUrl} | sh /dev/stdin --channel {channel} --install-dir {dotnetInstallPath}");
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
      /// Tries to install the .NET SDK assuming the target device has a direct internet connection.
      /// </summary>
      /// <returns>A <see cref="Task{Boolean}"/>representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      public async Task<bool> TryInstallDotNetRuntimeOnlineAsync(string channel, string runtime)
      {
         try
         {
            using (var commands = session.CreateCommandSession())
            {
               logger.WriteLineOutputExtensionPane("--------------------------------------------------");
               logger.WriteLineOutputExtensionPane("Installing .NET runtime Online\r\n");

               var dotnetInstallPath = configurationAggregator.QueryDotNetInstallFolderPath();
               var result = await commands.ExecuteCommandAsync($"curl -sSL {PackageConstants.Dotnet.GetInstallDotnetShUrl} | sh /dev/stdin --channel {channel} --runtime {runtime} --install-dir {dotnetInstallPath}");
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
   }
}
