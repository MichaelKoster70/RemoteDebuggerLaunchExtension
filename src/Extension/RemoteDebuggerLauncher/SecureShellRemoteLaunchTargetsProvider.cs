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
      private readonly IOptionsPageAccessor optionsPageAccessor;

      [ImportingConstructor]
      public SecureShellRemoteLaunchTargetsProvider(ConfiguredProject configuredProject, IOptionsPageAccessor optionsPageAccessor)
      {
         this.configuredProject = configuredProject;
         this.optionsPageAccessor = optionsPageAccessor;
      }

      public Task OnAfterLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         //DTE2 dte = (DTE2)RemoteDebuggerLauncherPackage.GetGlobalService(typeof(SDTE));
         //dte.ExecuteCommand("DebugAdapterHost.Launch", $"/LaunchJson:\"{profile.ExecutablePath}\"");
         return Task.CompletedTask;
      }

      public async Task OnBeforeLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         var configurationAggregator = ConfigurationAggregator.Create(profile, optionsPageAccessor);
         var outputPane = PackageHelper.GetGlobalService<IVsOutputWindowPane, SVsGeneralOutputWindowPane>();

         outputPane.OutputStringThreadSafe("Hello World");
         //return Task.CompletedTask;
      }

      public async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         var projectFolder = Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);
         var outputDir = await GetOutputDirectoryAsync(configuredProject);


         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         var configurationAggregator = ConfigurationAggregator.Create(profile, optionsPageAccessor);
         var launchSettings = CreateLaunchSettings(launchOptions, configurationAggregator);

         return new List<IDebugLaunchSettings>() { launchSettings};
      }

      private DebugLaunchSettings CreateLaunchSettings(DebugLaunchOptions launchOptions, ConfigurationAggregator configurationAggregator)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         var launchSettings = new DebugLaunchSettings(launchOptions)
         {
            LaunchOperation = DebugLaunchOperation.CreateProcess,
            Executable = "dotnet",
            Options = AdapterLaunchConfiguration.Create(configurationAggregator),
            LaunchDebugEngineGuid = PackageConstants.EngineGuid,
            Project = configuredProject.UnconfiguredProject.Services.HostObject as IVsHierarchy
         };

         return launchSettings;
      }

      public bool SupportsProfile(ILaunchProfile profile)
      {
         return profile.CommandName.Equals(PackageConstants.SecureShellRemoteLaunchCommandName);
      }

      private static Task<string> GetOutputDirectoryAsync(ConfiguredProject configuredProject)
      {
         var projectPropertiesProvider = configuredProject.Services.ProjectPropertiesProvider;

         ThrowIf.NotPresent(projectPropertiesProvider);
         return projectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync("OutDir");
      }
   }
}
