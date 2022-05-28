// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher.Extensions
{
   /// <summary>
   /// Extension method providing VS output pane extension methods
   /// </summary>
   internal static class IVsOutputWindowPaneExtensions
   {
      public static void OutputStringNoPump(this IVsOutputWindowPane pane, string pszOutputString)
      {
         Requires.NotNull(pane, nameof(pane));

         ThreadHelper.ThrowIfNotOnUIThread();

         if (pane is IVsOutputWindowPaneNoPump noPumpPane)
         {
            noPumpPane.OutputStringNoPump(pszOutputString);
         }
         else
         {
            Verify.HResult(pane.OutputStringThreadSafe(pszOutputString));
         }
      }
   }
}
