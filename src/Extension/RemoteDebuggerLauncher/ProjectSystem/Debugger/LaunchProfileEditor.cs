// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
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
   [AppliesTo(PackageConstants.CPS.AppliesTo.LaunchProfilesAndCps)]
   internal class LaunchProfileEditor : ILaunchProfileEditor
   {
      private readonly ILaunchSettingsProvider launchSettingsProvider;
      private readonly UnconfiguredProject unconfiguredProject;

      [ImportingConstructor]
      public LaunchProfileEditor(ILaunchSettingsProvider launchSettingsProvider, UnconfiguredProject unconfiguredProject)
      {
         this.launchSettingsProvider = launchSettingsProvider ?? throw new ArgumentNullException(nameof(launchSettingsProvider));
         this.unconfiguredProject = unconfiguredProject ?? throw new ArgumentNullException(nameof(unconfiguredProject));
      }

      /// <inheritdoc />
      public async Task<bool> UpdateProfilePropertyAsync(string propertyName, string propertyValue)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         try
         {
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
               otherSettings: updatedOtherSettings,
               inMemoryProfile: false
            );

            // Update and save the settings using the CPS API
            await launchSettingsProvider.AddOrUpdateProfileAsync(updatedProfile, addToFront: false);

            return true;
         }
         catch (Exception)
         {
            // Log or handle the exception as needed
            return false;
         }
      }
   }
}
