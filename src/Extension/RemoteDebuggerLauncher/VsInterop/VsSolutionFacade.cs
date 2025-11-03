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
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Implements The facade for the loaded Visual Studio solution.
   /// Implements the <see cref="IVsSolutionFacade" />
   /// </summary>
   /// <seealso cref="IVsSolutionFacade" />
   internal class VsSolutionFacade : IVsSolutionFacade
   {
      private readonly IAsyncServiceProvider serviceProvider;
      private readonly VsSolutionEventsListener solutionEventsListener;

      /// <summary>
      /// Initializes a new instance of the <see cref="VsSolutionFacade"/> class.
      /// </summary>
      /// <param name="serviceProvider">The service provider.</param>
      public VsSolutionFacade(IServiceProvider serviceProvider)
      {
         this.serviceProvider = serviceProvider as IAsyncServiceProvider;

         // Register the solution event listener
          solutionEventsListener = new VsSolutionEventsListener(serviceProvider.GetService<SVsSolution, IVsSolution>());
      }

      public IVsSolutionEventsFacade EventsFacade { get { return solutionEventsListener; } }

      /// <inheritdoc />
      public async Task<IList<IUnconfiguredPackageServiceFactory>> GetActiveConfiguredProjectsAsync()
      {
         var dte = await serviceProvider.GetAutomationModelTopLevelObjectServiceAsync();
         var projectService = await serviceProvider.GetProjectServiceAsync();

         var activeConfiguredProjects = new List<IUnconfiguredPackageServiceFactory>();

         // the CPS service must be available for this to succeed
         if (projectService == null)
         {
            return activeConfiguredProjects;
         }

         // get the configured startup projects
         var startupProjects = await dte.GetSolutionStartupProjectsAsync();
         if (startupProjects.Count == 0)
         {
            return activeConfiguredProjects;
         }

         // Get the collection of startup projects
         var unconfiguredProjects = projectService.LoadedUnconfiguredProjects.Where(u => startupProjects.Any(s => u.FullPath.EndsWith(s))).ToList();
         if (unconfiguredProjects.Count == 0)
         {
            return activeConfiguredProjects;
         }

         // we have at least one startup project
         foreach (var unconfiguredProject in unconfiguredProjects)
         {
            var factory = unconfiguredProject.Services.ExportProvider.GetService<IUnconfiguredPackageServiceFactory>();
            if (factory != null && await factory.ConfigureAsync())
            {
               activeConfiguredProjects.Add(factory);
            }
         }

         return activeConfiguredProjects;
      }
   }
}
