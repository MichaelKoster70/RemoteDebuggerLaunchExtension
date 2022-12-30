﻿// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

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
      private readonly IDebugTokenReplacer tokenReplacer;

      [ImportingConstructor]
      public SecureShellRemoteLaunchTargetsProvider(ConfiguredProject configuredProject, IProjectThreadingService threadingService, IDebugTokenReplacer tokenReplacer)
      {
         this.configuredProject = configuredProject;
         this.threadingService = threadingService;
         this.tokenReplacer = tokenReplacer;
      }

      /// <summary>
      /// Called right after launch to allow the provider to do additional work.
      /// </summary>
      public Task OnAfterLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         // no actions needed
         return Task.CompletedTask;
      }

      /// <summary>
      /// Called just prior to launch to allow the provider to do additional work.
      /// </summary>
      /// <remarks>
      /// Performs additional steps like installing the debugger, publish the app, or deploy data to the target device.
      /// </remarks>
      public async Task OnBeforeLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         var optionsPageAccessor = await AsyncServiceProvider.GlobalProvider.GetOptionsPageServiceAsync().ConfigureAwait(true);
         var loggerService = await AsyncServiceProvider.GlobalProvider.GetLoggerServiceAsync().ConfigureAwait(true);
         var statusbarService = await AsyncServiceProvider.GlobalProvider.GetStatusbarServiceAsync().ConfigureAwait(true);

         await TaskScheduler.Default;

         // get environment variables and msbuild properties resolved
         var resolveddProfile = await tokenReplacer.ReplaceTokensInProfileAsync(profile).ConfigureAwait(false);

         var configurationAggregator = ConfigurationAggregator.Create(resolveddProfile, optionsPageAccessor);
         var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService, statusbarService);
         var publishService = Publish.Create(configuredProject, loggerService, statusbarService);

         await threadingService.JoinableTaskFactory.SwitchToMainThreadAsync();

         // Step 1: try to connect to the device
         await remoteOperations.CheckConnectionThrowAsync().ConfigureAwait(true);

         // Step 2: try to install the latest debugger version
         bool installDebugger = configurationAggregator.QueryInstallDebuggerOnDeploy();
         if (installDebugger)
         {
            // only install the debugger if configured in launch profile
            var succeeded = await remoteOperations.TryInstallVsDbgOnlineAsync().ConfigureAwait(true);
            if (!succeeded)
            {
               await remoteOperations.TryInstallVsDbgOfflineAsync().ConfigureAwait(true);
            }
         }

         // Step 3: run the publishing if requested to do so
         if (configurationAggregator.QueryPublishOnDeploy())
         {
            await publishService.StartAsync().ConfigureAwait(true);
         }

         // Step 4: Deploy application to target folder
         var outputPath = await publishService.GetOutputDirectoryPathAsync().ConfigureAwait(true);
         await remoteOperations.DeployRemoteFolderAsync(outputPath, true).ConfigureAwait(true);

         statusbarService.SetText(Resources.RemoteCommandLanchingDebugger);
      }

      /// <summary>
      /// Called in response to an F5/Ctrl+F5 operation to get the debug launch settings to pass to the debugger for the active profile.
      /// </summary>
      /// <param name="launchOptions"></param>
      /// <param name="profile"></param>
      /// <returns></returns>
      public async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         var optionsPageAccessor = await AsyncServiceProvider.GlobalProvider.GetOptionsPageServiceAsync().ConfigureAwait(true);
         var loggerService = await AsyncServiceProvider.GlobalProvider.GetLoggerServiceAsync().ConfigureAwait(true);
         var statusbarService = await AsyncServiceProvider.GlobalProvider.GetStatusbarServiceAsync().ConfigureAwait(true);

         await TaskScheduler.Default;

         // get environment variables and msbuild properties resolved
         var resolveddProfile = await tokenReplacer.ReplaceTokensInProfileAsync(profile).ConfigureAwait(false);

         var configurationAggregator = ConfigurationAggregator.Create(resolveddProfile, optionsPageAccessor);

         var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService, statusbarService);

         // the rest must be executed on the UI thread
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         // validate that target is reachable
         await remoteOperations.CheckConnectionThrowAsync().ConfigureAwait(true);

         var launchSettings = new DebugLaunchSettings(launchOptions)
         {
            LaunchOperation = DebugLaunchOperation.CreateProcess,
            Executable = "dotnet",
            Options = await AdapterLaunchConfiguration.CreateFrameworkDependantAsync(configurationAggregator, configuredProject, loggerService, remoteOperations).ConfigureAwait(true),
            LaunchDebugEngineGuid = PackageConstants.DebugLaunchSettings.EngineGuid,
            Project = configuredProject.UnconfiguredProject.Services.HostObject as IVsHierarchy
         };

         return new List<IDebugLaunchSettings>() { launchSettings };
      }

      /// <summary>
      /// Return true if the provider supports the suplied profile type.
      /// </summary>
      /// <param name="profile">The profile to validate.</param>
      public bool SupportsProfile(ILaunchProfile profile)
      {
         return profile.CommandName.Equals(PackageConstants.LaunchProfile.CommandName);
      }
   }
}
