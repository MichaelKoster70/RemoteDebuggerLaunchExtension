// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Logging Service implementation writing to a custom Visual Studio Output Window.
   /// Implements the <see cref="RemoteDebuggerLauncher.ILoggerService" />
   /// </summary>
   /// <seealso cref="RemoteDebuggerLauncher.ILoggerService" />
   internal class LoggerService : SLoggerService, ILoggerService
   {
      private readonly IVsOutputWindow outputWindow;

      /// <summary>
      /// Initializes a new instance of the <see cref="LoggerService"/> class.
      /// </summary>
      public LoggerService()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
      }

      /// <inheritdoc />
      public void WriteOutputDebugPane(string message, bool activate)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         var pane = EnsurePane(VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Debug", activate);
         pane.OutputStringThreadSafe(message);
      }


      /// <inheritdoc />
      public void WriteOutputExtensionPane(string message, bool activate)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         var pane = EnsurePane(PackageConstants.OutputWindow.OutputPaneGuid, PackageConstants.OutputWindow.OutputPaneName, activate);
         pane.OutputStringThreadSafe(message);
      }

      /// <inheritdoc />
      public void WriteOutputExtensionPane(bool predicate, string message, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         if (predicate)
         {
            WriteLineOutputExtensionPane(message, activate);
         }
      }

      /// <inheritdoc />
      public void WriteLineOutputExtensionPane(string message, bool activate)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         var pane = EnsurePane(PackageConstants.OutputWindow.OutputPaneGuid, PackageConstants.OutputWindow.OutputPaneName, activate);
         pane.OutputStringThreadSafe(message + "\r\n");
      }

      private IVsOutputWindowPane EnsurePane(Guid guid, string name, bool activate)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         int result = outputWindow.GetPane(ref guid, out IVsOutputWindowPane pane);
         if (result != VSConstants.S_OK)
         {
            result = outputWindow.CreatePane(ref guid, name, 1, 1);
            ErrorHandler.ThrowOnFailure(result);

            result = outputWindow.GetPane(ref guid, out pane);
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
