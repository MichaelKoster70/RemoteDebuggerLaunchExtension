﻿// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Composition;
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
         var optionsPageAccessor = await AsyncServiceProvider.GlobalProvider.GetOptionsPageServiceAsync();
         var loggerService = await AsyncServiceProvider.GlobalProvider.GetLoggerServiceAsync();
         var statusbarService = await AsyncServiceProvider.GlobalProvider.GetStatusbarServiceAsync();
         var waitDialogFactory = await AsyncServiceProvider.GlobalProvider.GetWaitDialogFactoryAsync();

         // get environment variables and msbuild properties resolved
         var resolveddProfile = await tokenReplacer.ReplaceTokensInProfileAsync(profile);

         var configurationAggregator = ConfigurationAggregator.Create(resolveddProfile, optionsPageAccessor);
         var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService, statusbarService);
         var publishService = Publish.Create(configuredProject, loggerService, waitDialogFactory);

         // the rest must be executed on the UI thread
         await threadingService.JoinableTaskFactory.SwitchToMainThreadAsync();

         // Step 1: try to connect to the device
         await remoteOperations.CheckConnectionThrowAsync(false);

         // Step 2: try to install the latest debugger version
         bool installDebugger = configurationAggregator.QueryInstallDebuggerOnDeploy();
         if (installDebugger)
         {
            // only install the debugger if configured in launch profile
            var succeeded = await remoteOperations.TryInstallVsDbgOnlineAsync();
            if (!succeeded)
            {
               await remoteOperations.TryInstallVsDbgOfflineAsync();
            }
         }

         // Step 3: run the publishing if requested
         if (configurationAggregator.QueryPublishOnDeploy())
         {
            await publishService.StartAsync();
         }

         // Step 4: Deploy application to target folder
         var outputPath = await publishService.GetOutputDirectoryPathAsync();
         await remoteOperations.DeployRemoteFolderAsync(outputPath, true);

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
         var optionsPageAccessor = await AsyncServiceProvider.GlobalProvider.GetOptionsPageServiceAsync();
         var loggerService = await AsyncServiceProvider.GlobalProvider.GetLoggerServiceAsync();
         var statusbarService = await AsyncServiceProvider.GlobalProvider.GetStatusbarServiceAsync();

         // get environment variables and msbuild properties resolved
         var resolveddProfile = await tokenReplacer.ReplaceTokensInProfileAsync(profile);

         var configurationAggregator = ConfigurationAggregator.Create(resolveddProfile, optionsPageAccessor);
         var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService, statusbarService);

         // the rest must be executed on the UI thread
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         try
         {
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
         catch
         {
            statusbarService.Clear();
            throw;
         }
      }

      /// <summary>
      /// Return true if the provider supports the suplied profile type.
      /// </summary>
      /// <param name="profile">The profile to validate.</param>
      public bool SupportsProfile(ILaunchProfile profile)
      {
         return profile.CommandName.Equals(PackageConstants.LaunchProfile.CommandName, StringComparison.Ordinal);
      }
   }
}
