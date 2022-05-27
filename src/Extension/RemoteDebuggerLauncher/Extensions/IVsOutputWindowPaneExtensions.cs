// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher.Extensions
{
   internal static class IVsOutputWindowPaneExtensions
   {
      public static void OutputStringNoPump(this IVsOutputWindowPane pane, string pszOutputString)
      {
         Requires.NotNull(pane, nameof(pane));

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
