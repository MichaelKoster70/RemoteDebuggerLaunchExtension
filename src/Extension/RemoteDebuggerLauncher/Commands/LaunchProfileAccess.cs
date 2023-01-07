// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using EnvDTE80;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class providing <see cref="ILaunchProfile"/> access services.
   /// </summary>
   internal class LaunchProfileAccess
   {
      private readonly DTE2 dte;
      private readonly IProjectService projectService;

      /// <summary>
      /// Initializes a new instance of the <see cref="LaunchProfileAccess"/> class.
      /// </summary>
      /// <param name="dte">The top level VS aurtomation model instance to use.</param>
      /// <param name="projectService">The CPS project service instance to use.</param>
      public LaunchProfileAccess(DTE2 dte, IProjectService projectService)
      {
         this.dte = dte;
         this.projectService = projectService;
      }

      /// <summary>
      /// Get all active launch profiles in the active VS session.
      /// </summary>
      /// <returns>A <see cref="Task{IList{ILaunchProfile}}"/> representing the asynchronous operation.</returns>
      public async Task<IList<ILaunchProfile>> GetActiveLaunchProfilesAsync()
      {
         var activeLaunchProfiles = new List<ILaunchProfile>();

         // the CPS service must be available for this to succeed
         if (projectService == null)
         {
            return activeLaunchProfiles;
         }

         // get the configured startup projects
         var startupProjects = await dte.GetSolutionStartupProjectsAsync();
         if (startupProjects.Count == 0)
         {
            // no launch profile, if we have no startup projects
            return activeLaunchProfiles;
         }

         // Get the collection of startup projects
         var unconfiguredProjects = projectService.LoadedUnconfiguredProjects.Where(u => startupProjects.Any(s => u.FullPath.EndsWith(s))).ToList();
         if (unconfiguredProjects.Count == 0)
         {
            return activeLaunchProfiles;
         }

         // we have at least one startup project
         foreach (var unconfiguredProject in unconfiguredProjects)
         {
            //Get the launch settings provider, if available
            var launchSettingsProvider = unconfiguredProject.Services.ExportProvider.GetService<ILaunchSettingsProvider>(true);
            if (launchSettingsProvider != null)
            {
               var activeProfile = launchSettingsProvider.ActiveProfile;
               if (activeProfile.CommandName.Equals(PackageConstants.LaunchProfile.CommandName, StringComparison.Ordinal))
               {
                  activeLaunchProfiles.Add(launchSettingsProvider.ActiveProfile);
               }
            }
         }

         return activeLaunchProfiles;
      }


      public async Task<IList<(ILaunchProfile LaunchProfile, ConfiguredProject ConfiguredProject)>> GetActiveLaunchProfilseWithProjectAsync()
      {
         var activeLaunchProfiles = new List<(ILaunchProfile LaunchProfile, ConfiguredProject ConfiguredProject)>();

         // the CPS service must be available for this to succeed
         if (projectService == null)
         {
            return activeLaunchProfiles;
         }

         // get the configured startup projects
         var startupProjects = await dte.GetSolutionStartupProjectsAsync();
         if (startupProjects.Count == 0)
         {
            // no launch profile, if we have no startup projects
            return activeLaunchProfiles;
         }

         // Get the collection of startup projects
         var unconfiguredProjects = projectService.LoadedUnconfiguredProjects.Where(u => startupProjects.Any(s => u.FullPath.EndsWith(s))).ToList();
         if (unconfiguredProjects.Count == 0)
         {
            return activeLaunchProfiles;
         }

         // we have at least one startup project
         foreach (var unconfiguredProject in unconfiguredProjects)
         {
            //Get the launch settings provider, if available
            var launchSettingsProvider = unconfiguredProject.Services.ExportProvider.GetService<ILaunchSettingsProvider>(true);
            if (launchSettingsProvider != null)
            {
               var activeProfile = launchSettingsProvider.ActiveProfile;
               if (activeProfile.CommandName.Equals(PackageConstants.LaunchProfile.CommandName, StringComparison.Ordinal))
               {
                  var confiuredProject = unconfiguredProject.LoadedConfiguredProjects.First();
                  activeLaunchProfiles.Add((launchSettingsProvider.ActiveProfile, confiuredProject));
               }
            }
         }

         return activeLaunchProfiles;
      }
   }
}
