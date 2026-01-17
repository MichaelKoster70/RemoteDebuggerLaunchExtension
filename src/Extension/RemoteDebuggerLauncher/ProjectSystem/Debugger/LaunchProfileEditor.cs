// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Service for editing launch profile settings using CPS ILaunchSettingsProvider.
   /// Implements <see cref="ILaunchProfileEditor"/>
   /// </summary>
   [Export(typeof(ILaunchProfileEditor))]
   [Shared(ExportContractNames.Scopes.UnconfiguredProject)]
   internal class LaunchProfileEditor : ILaunchProfileEditor
   {
      private readonly ILaunchSettingsProvider launchSettingsProvider;

      [ImportingConstructor]
      public LaunchProfileEditor(ILaunchSettingsProvider launchSettingsProvider)
      {
         this.launchSettingsProvider = launchSettingsProvider ?? throw new ArgumentNullException(nameof(launchSettingsProvider));
      }

      /// <inheritdoc />
      public string ProfileName => launchSettingsProvider.ActiveProfile?.Name ?? string.Empty;

      /// <inheritdoc />
      public async Task<bool> UpdateProfilePropertyAsync(string propertyName, string propertyValue)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         var activeProfile = launchSettingsProvider.ActiveProfile;
         if (activeProfile == null)
         {
            return false;
         }

         // Create updated OtherSettings with the new property value
         var updatedOtherSettings = activeProfile.OtherSettings != null
            ? activeProfile.OtherSettings.SetItem(propertyName, propertyValue)
            : ImmutableDictionary<string, object>.Empty.Add(propertyName, propertyValue);

         // Create a new launch profile with the updated settings using LaunchProfile class
         var updatedProfile = new LaunchProfile(
            name: activeProfile.Name,
            commandName: activeProfile.CommandName,
            executablePath: activeProfile.ExecutablePath,
            commandLineArgs: activeProfile.CommandLineArgs,
            workingDirectory: activeProfile.WorkingDirectory,
            launchBrowser: activeProfile.LaunchBrowser,
            launchUrl: activeProfile.LaunchUrl,
            environmentVariables: activeProfile.EnvironmentVariables,
            otherSettings: updatedOtherSettings);

         // Update and save the settings using the CPS API
         await launchSettingsProvider.AddOrUpdateProfileAsync(updatedProfile, addToFront: false);

         return true;
      }
   }
}
