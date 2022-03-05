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
         //DTE2 dte = (DTE2)RemoteDebuggerLauncherPackage.GetGlobalService(typeof(SDTE));
         //dte.ExecuteCommand("DebugAdapterHost.Launch", $"/LaunchJson:\"{profile.ExecutablePath}\"");
         return Task.CompletedTask;
      }

      public Task OnBeforeLaunchAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         return Task.CompletedTask;
      }

      public async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         DTE2 dte = (DTE2)Package.GetGlobalService(typeof(SDTE));

         var project = dte.Solution.GetStartupProject();

         var projectFolder = Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);
         var outputDir = await GetOutputDirectoryAsync(configuredProject);


         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         var launchSettings = CreateLaunchSettings(launchOptions, profile);

         //ConfiguredProject.

//         debugTargetInfoValue.Options = real value should probably come from launchSettings.json, with maybe a few other things tacked on, but we want something like:
//{
//            "name": ".NET Core Launch",
//  "request": "launch"
//  "type": "coreclr",
//  "$adapter": "c:\\mytools\\plink.exe",
//  "$adapterArgs": "-i c:\\users\\greggm\\ssh-key.ppk greggm@mylinuxbox -batch -T ~/vsdbg/vsdbg --interpreter=vscode",
//  "cwd": "~/clicon",
//  "program": "bin/Debug/netcoreapp1.0/clicon.dll",
//}
         //debugTargetInfoValue.Project = configuredProject.UnconfiguredProject.Services.HostObject as IVsHierarchy;
         return Array.Empty<IDebugLaunchSettings>();
      }

      private DebugLaunchSettings CreateLaunchSettings(DebugLaunchOptions launchOptions, ILaunchProfile profile)
      {
         ThreadHelper.ThrowIfNotOnUIThread();


         var launchSettings = new DebugLaunchSettings(launchOptions)
         {
            LaunchOperation = DebugLaunchOperation.CreateProcess,
            Executable = "dotnet",
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
