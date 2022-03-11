// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using EnvDTE80;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// This class provides a
   /// Implements the <see cref="Microsoft.VisualStudio.ProjectSystem.VS.Debug.IDebugProfileLaunchTargetsProvider" />
   /// </summary>
   /// <seealso cref="Microsoft.VisualStudio.ProjectSystem.VS.Debug.IDebugProfileLaunchTargetsProvider" />
   [Export(typeof(IDebugProfileLaunchTargetsProvider))]
   [AppliesTo(PackageConstants.AppliesToLaunchProfiles)]
   [Order(50)]
   internal class SecureShellRemoteLaunchTargetsProvider : IDebugProfileLaunchTargetsProvider
   {
      private readonly ConfiguredProject configuredProject;

      [ImportingConstructor]
      public SecureShellRemoteLaunchTargetsProvider(ConfiguredProject configuredProject)
      {
         this.configuredProject = configuredProject;
      }

      public Task OnAfterLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         return Task.CompletedTask;
      }

      public async Task OnBeforeLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         var optionsPageAccessor = await ServiceProvider.GetGlobalServiceAsync<SOptionsPageAccessor, IOptionsPageAccessor>();
         var loggerService = await ServiceProvider.GetGlobalServiceAsync<SLoggerService, ILoggerService>();
         var configurationAggregator = ConfigurationAggregator.Create(profile, optionsPageAccessor);

         using (var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService))
         {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // try to connect to the device
            await remoteOperations.CheckConnectionThrowAsync().ConfigureAwait(true);

         }


         var projectPropertiesProvider = configuredProject.Services.ProjectPropertiesProvider;
         var properties = await projectPropertiesProvider.GetCommonProperties().GetPropertyNamesAsync();

         var projectFolder = Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);
         var outputDir = await configuredProject.GetOutputDirectoryAsync();
         var assemblyName = await configuredProject.GetAssemblyNameAsync();

         //return Task.CompletedTask;
      }

      public async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         var optionsPageAccessor = await AsyncServiceProvider.GlobalProvider.GetServiceAsync<SOptionsPageAccessor, IOptionsPageAccessor>();
         var configurationAggregator = ConfigurationAggregator.Create(profile, optionsPageAccessor);
         var launchSettings = await CreateLaunchSettingsAsync(launchOptions, configurationAggregator);

         return new List<IDebugLaunchSettings>() { launchSettings};
      }

      private async Task<DebugLaunchSettings> CreateLaunchSettingsAsync(DebugLaunchOptions launchOptions, ConfigurationAggregator configurationAggregator)
      {
         // must be executed on the UI thread
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         var launchSettings = new DebugLaunchSettings(launchOptions)
         {
            LaunchOperation = DebugLaunchOperation.CreateProcess,
            Executable = "dotnet",
            Options = await AdapterLaunchConfiguration.CreateAsync(configurationAggregator, configuredProject),
            LaunchDebugEngineGuid = PackageConstants.EngineGuid,
            Project = configuredProject.UnconfiguredProject.Services.HostObject as IVsHierarchy
         };

         return launchSettings;
      }

      public bool SupportsProfile(ILaunchProfile profile)
      {
         return profile.CommandName.Equals(PackageConstants.SecureShellRemoteLaunchCommandName);
      }
   }
}
