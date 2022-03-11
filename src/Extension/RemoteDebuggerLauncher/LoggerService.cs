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
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   internal class LoggerService : SLoggerService, ILoggerService
   {
      //private DTE2 dte;
      private readonly IVsOutputWindow outputWindow;

      public LoggerService()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
      }

      /// <summary>
      /// Writes the supplied message to the VS debug output pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      public void WriteOutputDebugPane(string message)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         var pane = EnsurePane(VSConstants.OutputWindowPaneGuid.DebugPane_guid, true);
         pane.OutputStringThreadSafe(message);
      }

      private IVsOutputWindowPane EnsurePane(Guid guid, bool activate)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         IVsOutputWindowPane pane;

         int result = outputWindow.GetPane(ref guid, out pane);
         if (result != VSConstants.S_OK)
         {
            result = outputWindow.CreatePane(ref guid, "Debug", 1, 1);
            ErrorHandler.ThrowOnFailure(result);
         }

         if (activate)
         {
            pane.Activate();
         }
         return pane;
      }
   }
}
