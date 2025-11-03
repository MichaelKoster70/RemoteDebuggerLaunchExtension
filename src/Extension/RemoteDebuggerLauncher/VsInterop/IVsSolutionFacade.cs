// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface for a facade for the loaded Visual Studio solution.
   /// </summary>
   internal interface IVsSolutionFacade
   {
      /// <summary>
      /// Gets the Solution events facade.
      /// </summary>
      /// <value>The <see cref="IVsSolutionEventsFacade"/> instance.</value>
      IVsSolutionEventsFacade EventsFacade { get; }

      /// <summary>
      /// Gets the active (startup) configured projects.
      /// </summary>
      /// <returns>A collection of the active projects.Never null.</returns>
      Task<IList<IUnconfiguredPackageServiceFactory>> GetActiveConfiguredProjectsAsync();
   }
}
