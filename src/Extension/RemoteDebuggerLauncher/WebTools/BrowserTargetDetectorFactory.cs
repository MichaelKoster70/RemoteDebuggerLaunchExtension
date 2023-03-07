// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Utility class providing a browser target detector.
   /// </summary>
   internal static class BrowserTargetDetectorFactory
   {
      private static readonly List<BrowserTargetDetector> supportedDebugTargets = new List<BrowserTargetDetector>()
      {
         new MsEdgeBrowserTargetDetector(),
         new ChromeBrowserTargetDetector()
      };

      /// <summary>
      /// Gets the browser debug target based on the supplied executable name.
      /// </summary>
      /// <param name="executable">The executable name.</param>
      /// <returns>A <see cref="BrowserDebugTarget"/>. Never null.</returns>
      public static BrowserTargetDetector GetDebugTargetByLauncher(string executable) => supportedDebugTargets.FirstOrDefault((x) => x.MatchesLauncher(executable)) ?? new DefaultBrowserTargetDetector(executable);
   }
}
