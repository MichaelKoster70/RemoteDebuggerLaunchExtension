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

namespace RemoteDebuggerLauncher
{
   internal static class VsInteropExtensions
   {
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
