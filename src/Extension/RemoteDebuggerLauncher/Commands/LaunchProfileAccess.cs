// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
      readonly IProjectService projectService;

      public LaunchProfileAccess(DTE2 dte, IProjectService projectService)
      {
         this.dte = dte;
         this.projectService = projectService;
      }

      /// <summary>
      /// Get all active launch profiles in the  active VS session.
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
         var startupProjects = await dte.GetSolutionStartupProjectsAsync().ConfigureAwait(true);
         if (startupProjects.Count == 0)
         {
            // no launch profile, if we have no startup projects
            return activeLaunchProfiles;
         }

         // Get the collection of startup projects
         var unconfiguredProjects = projectService.LoadedUnconfiguredProjects.Where(u => startupProjects.Where(s => u.FullPath.EndsWith(s)).Count() > 0).ToList();
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
               if (activeProfile.CommandName.Equals(PackageConstants.LaunchProfile.CommandName))
               {
                  activeLaunchProfiles.Add(launchSettingsProvider.ActiveProfile);
               }
            }
         }

         return activeLaunchProfiles;
      }
   }
}
