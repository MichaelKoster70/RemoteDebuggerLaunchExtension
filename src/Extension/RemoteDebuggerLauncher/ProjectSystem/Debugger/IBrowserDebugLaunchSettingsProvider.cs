// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining the debug launch settings provider for VS provided web browser support.
   /// </summary>
   internal interface IBrowserDebugLaunchSettingsProvider
   {
      /// <summary>
      /// gets the launcg settings based on the supplied parameters.
      /// </summary>
      /// <param name="browseUri">The target to browse to.</param>
      /// <param name="launchOptions">The debug launch options.</param>
      /// <param name="configuration">The launch profile configuration.</param>
      /// <returns>A list of <see cref="DebugLaunchSettings"/> if browser are to be launched; else an empty list.</returns>
      Task<IList<DebugLaunchSettings>> GetLaunchSettingsAsync(Uri browseUri, DebugLaunchOptions launchOptions, ConfigurationAggregator configuration);
   }
}
