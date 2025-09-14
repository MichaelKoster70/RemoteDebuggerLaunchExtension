// ----------------------------------------------------------------------------
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
using Microsoft.VisualStudio.Shell.Interop;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// This class provides a custom Launch Profile that deploys and launches the VS Code debugger on a Linux based remote device.
   /// Implements the <see cref=IDebugProfileLaunchTargetsProvider" />
   /// </summary>
   /// <seealso cref="IDebugProfileLaunchTargetsProvider" />
   [Export(typeof(IDebugProfileLaunchTargetsProvider))]
   [Shared(ExportContractNames.Scopes.ConfiguredProject)]
   [AppliesTo(PackageConstants.CPS.AppliesTo.LaunchProfilesAndCps)]
   [Order(50)]
   internal class SecureShellRemoteLaunchTargetsProvider : IDebugProfileLaunchTargetsProvider
   {
      private readonly ConfiguredProject configuredProject;
      private readonly IProjectThreadingService threadingService;
      private readonly IConfiguredPackageServiceFactory packageServiceFactory;
      private readonly IBrowserDebugLaunchSettingsProvider browserDebugLaunch;

      [ImportingConstructor]
      public SecureShellRemoteLaunchTargetsProvider(ConfiguredProject configuredProject, IProjectThreadingService threadingService, IConfiguredPackageServiceFactory packageServiceFactory, IBrowserDebugLaunchSettingsProvider browserDebugLaunch)
      {
         this.configuredProject = configuredProject;
         this.threadingService = threadingService;
         this.packageServiceFactory = packageServiceFactory;
         this.browserDebugLaunch = browserDebugLaunch;
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
         // get services
         await packageServiceFactory.ConfigureAsync(profile);

         var statusbarService = await packageServiceFactory.GetStatusbarServiceAsync();
         var remoteOperations = await packageServiceFactory.GetSecureShellRemoteOperationsAsync();
         var deployService = await packageServiceFactory.GetDeployServiceAsync(true);

         // the rest must be executed on the UI thread
         await threadingService.JoinableTaskFactory.SwitchToMainThreadAsync();

         // Step 1: try to connect to the device
         await remoteOperations.CheckConnectionThrowAsync(false);

         // Step 2: try to install the latest debugger version
         bool installDebugger = packageServiceFactory.Configuration.QueryInstallDebuggerOnStartDebugging();
         if (installDebugger)
         {
            // only install the debugger if configured in launch profile
            var succeeded = await remoteOperations.TryInstallVsDbgOnlineAsync();
            if (!succeeded)
            {
               await remoteOperations.TryInstallVsDbgOfflineAsync();
            }
         }

         // Step 3: deploy with publish
         bool deploy = packageServiceFactory.Configuration.QueryDeployOnStartDebugging();
         if (deploy)
         {
            await deployService.DeployAsync(false);
         }

         statusbarService.SetText(Resources.RemoteCommandLaunchingDebugger);
      }

      /// <summary>
      /// Called in response to an F5/Ctrl+F5 operation to get the debug launch settings to pass to the debugger for the active profile.
      /// </summary>
      /// <param name="launchOptions"></param>
      /// <param name="profile"></param>
      /// <returns></returns>
      public async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         // get services 
         await packageServiceFactory.ConfigureAsync(profile);

         var statusbar = await packageServiceFactory.GetStatusbarServiceAsync();
         var remoteOperations = await packageServiceFactory.GetSecureShellRemoteOperationsAsync();

         // the rest must be executed on the UI thread
         await threadingService.JoinableTaskFactory.SwitchToMainThreadAsync();

         try
         {
            packageServiceFactory.OutputPane.WriteLine(Resources.RemoteCommandLaunchingDebuggerOutputPaneStart, configuredProject.GetProjectName(), configuredProject.GetConfiguration());

            // validate that target is reachable
            await remoteOperations.CheckConnectionThrowAsync();

            var debugLaunchSettings = new List<IDebugLaunchSettings>();

            await AddSshLaunchTargetAsync(debugLaunchSettings, launchOptions, packageServiceFactory);
            await AddLaunchBrowserTargetAsync(debugLaunchSettings, launchOptions, packageServiceFactory);

            return debugLaunchSettings;
         }
         catch
         {
            statusbar.Clear();
            throw;
         }
      }

      /// <summary>
      /// Return true if the provider supports the supplied profile type.
      /// </summary>
      /// <param name="profile">The profile to validate.</param>
      public bool SupportsProfile(ILaunchProfile profile)
      {
         return profile.IsSecureShellRemoteLaunch();
      }

      private async Task AddSshLaunchTargetAsync(List<IDebugLaunchSettings> debugLaunchSettings, DebugLaunchOptions launchOptions, IPackageServiceFactory factory)
      {
         var remoteOperations = await factory.GetSecureShellRemoteOperationsAsync();

         await threadingService.JoinableTaskFactory.SwitchToMainThreadAsync();


         var publishOnDeploy = factory.Configuration.QueryPublishOnDeploy();
         var publishMode = factory.Configuration.QueryPublishMode();

         var launchSettings = new DebugLaunchSettings(launchOptions)
         {
            LaunchOperation = DebugLaunchOperation.CreateProcess,
            Executable = "dotnet",
            LaunchDebugEngineGuid = PackageConstants.DebugLaunchSettings.EngineGuid,
            Project = configuredProject.UnconfiguredProject.Services.HostObject as IVsHierarchy
         };
         
         if (publishOnDeploy && publishMode == PublishMode.SelfContained)
         {
            launchSettings.Options = await AdapterLaunchConfiguration.CreateSelfContainedAsync(factory.Configuration, configuredProject, factory.OutputPane, remoteOperations);
         }
         else
         {
            // in all other cases, debug as framework dependent
            launchSettings.Options = await AdapterLaunchConfiguration.CreateFrameworkDependantAsync(factory.Configuration, configuredProject, factory.OutputPane, remoteOperations);
         }

         debugLaunchSettings.Add(launchSettings);
      }

      private async Task AddLaunchBrowserTargetAsync(List<IDebugLaunchSettings> debugLaunchSettings, DebugLaunchOptions launchOptions, IPackageServiceFactory factory)
      {
         await threadingService.JoinableTaskFactory.SwitchToMainThreadAsync();

         if (factory.Configuration.QueryLaunchBrowser())
         {
            var browserUri = factory.Configuration.QueryBrowserLaunchUri();
            if (browserUri == null)
            {
               factory.OutputPane.WriteLine(Resources.LaunchTargetsProviderNoLaunchUrl);
               return;
            }

            var launchSettings = await browserDebugLaunch.GetLaunchSettingsAsync(browserUri, launchOptions, factory.Configuration);

            debugLaunchSettings.AddRange(launchSettings);
         }
      }
   }
}
