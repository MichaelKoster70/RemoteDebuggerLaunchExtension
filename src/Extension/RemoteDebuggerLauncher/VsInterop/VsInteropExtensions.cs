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
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Extension Methods the <see cref="DTE2"/> interface.
   /// </summary>
   internal static class VsInteropExtensions
   {
      /// <summary>
      /// Gets the automation model top level object service (aka DTE2).
      /// </summary>
      /// <param name="serviceProvider">The service provider to query.</param>
      /// <returns>The <see cref="DTE2"/> service interface. Never null.</returns>
      public static Task<DTE2> GetAutomationModelTopLevelObjectServiceAsync(this IAsyncServiceProvider serviceProvider)
      {
         return serviceProvider.GetServiceAsync<SDTE, DTE2>();
      }

      /// <summary>
      /// Gets names of the startup projects in the loaded solution.
      /// </summary>
      /// <param name="dte">The DTE.</param>
      /// <returns>A collection holding the startup projects.</returns>
      public static async Task<IList<string>> GetSolutionStartupProjectsAsync(this DTE2 dte)
      {
         ThrowIf.ArgumentNull(dte, nameof(dte));

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         var startupProjects = (dte.Solution.SolutionBuild.StartupProjects as Object[]).Select(o => o as string).ToList();

         return startupProjects;
      }
   }
}
