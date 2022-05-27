// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// This class provides a custom Launch Profile that deploys and launches the VS Code debugger on a Linux based remote device.
   /// Implements the <see cref=IDebugProfileLaunchTargetsProvider" />
   /// </summary>
   /// <seealso cref="IDebugProfileLaunchTargetsProvider" />
   [Export(typeof(IDebugProfileLaunchTargetsProvider))]
   [AppliesTo(PackageConstants.LaunchProfile.AppliesTo + " + CPS")]
   [Order(50)]
   internal class SecureShellRemoteLaunchTargetsProvider : IDebugProfileLaunchTargetsProvider
   {
      private readonly ConfiguredProject configuredProject;
      private readonly IProjectThreadingService threadingService;

      [ImportingConstructor]
      public SecureShellRemoteLaunchTargetsProvider(ConfiguredProject configuredProject, IProjectThreadingService threadingService)
      {
         this.configuredProject = configuredProject;
         this.threadingService = threadingService;
      }

      public Task OnAfterLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         // no actions needed
         return Task.CompletedTask;
      }

      public async Task OnBeforeLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         var optionsPageAccessor = await ServiceProvider.GetGlobalServiceAsync<SOptionsPageAccessor, IOptionsPageAccessor>();
         var loggerService = await AsyncServiceProvider.GlobalProvider.GetServiceAsync<SLoggerService, ILoggerService>();
         var statusbarService = await AsyncServiceProvider.GlobalProvider.GetServiceAsync<SStatusbarService, IStatusbarService>();

         var configurationAggregator = ConfigurationAggregator.Create(profile, optionsPageAccessor);
         var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService, statusbarService);

         await threadingService.JoinableTaskFactory.SwitchToMainThreadAsync();

         // Step 1: try to connect to the device
         await remoteOperations.CheckConnectionThrowAsync();

         // Step 2: try to install the latest debugger version
         var succeeded = await remoteOperations.TryInstallVsDbgOnlineAsync().ConfigureAwait(true);
         if (!succeeded)
         {
            await remoteOperations.TryInstallVsDbgOfflineAsync().ConfigureAwait(true);
         }

         // Step 3: Deploy application to target folder
         var outputPath = await configuredProject.GetOutputDirectoryPathAsync();
         await remoteOperations.DeployRemoteFolderAsync(outputPath, true);
      }

      public async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         var optionsPageAccessor = await AsyncServiceProvider.GlobalProvider.GetServiceAsync<SOptionsPageAccessor, IOptionsPageAccessor>();
         var loggerService = await AsyncServiceProvider.GlobalProvider.GetServiceAsync<SLoggerService, ILoggerService>();
         var statusbarService = await AsyncServiceProvider.GlobalProvider.GetServiceAsync<SStatusbarService, IStatusbarService>();

         var configurationAggregator = ConfigurationAggregator.Create(profile, optionsPageAccessor);

         var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService, statusbarService);

         // the rest must be executed on the UI thread
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         // validate that target is reachable
         await remoteOperations.CheckConnectionThrowAsync();

         var launchSettings = new DebugLaunchSettings(launchOptions)
         {
            LaunchOperation = DebugLaunchOperation.CreateProcess,
            Executable = "dotnet",
            Options = await AdapterLaunchConfiguration.CreateFrameworkDependantAsync(configurationAggregator, configuredProject, loggerService, remoteOperations),
            LaunchDebugEngineGuid = PackageConstants.DebugLaunchSettings.EngineGuid,
            Project = configuredProject.UnconfiguredProject.Services.HostObject as IVsHierarchy
         };

         return new List<IDebugLaunchSettings>() { launchSettings };
      }

      public bool SupportsProfile(ILaunchProfile profile)
      {
         return profile.CommandName.Equals(PackageConstants.LaunchProfile.CommandName);
      }
   }
}
