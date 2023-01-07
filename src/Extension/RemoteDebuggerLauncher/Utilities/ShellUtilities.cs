// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utiliy class providing VS shell related services.
   /// </summary>
   internal static class ShellUtilities
   {
      /// <summary>
      /// Shows a Error Message Box
      /// </summary>
      /// <param name="serviceProvider">The service provider to be used for servic lookups.</param>
      /// <param name="message">The message to be displayed.</param>
      public static void ShowErrorMessageBox(IServiceProvider serviceProvider, string message)
      {
         _= VsShellUtilities.ShowMessageBox(serviceProvider, message, Resources.RemoteCommandInstallDebuggerCommandCaption, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      }
   }
}
