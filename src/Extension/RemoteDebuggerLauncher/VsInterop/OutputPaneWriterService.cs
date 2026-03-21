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
using Microsoft.VisualStudio.Threading;
using RemoteDebuggerLauncher.Extensions;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Logging Service implementation writing to a custom Visual Studio Output Window Pane.
   /// Implements the <see cref="IOutputPaneWriterService" />
   /// </summary>
   /// <seealso cref="IOutputPaneWriterService" />
   internal class OutputPaneWriterService : IOutputPaneWriterService
   {
      private readonly IVsOutputWindow outputWindow;

      /// <summary>
      /// Initializes a new instance of the <see cref="LoggerService"/> class.
      /// </summary>
      public OutputPaneWriterService()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
      }

      /// <inheritdoc />
      public void Write(string message, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         var pane = EnsurePane(PackageConstants.OutputWindow.OutputPaneGuid, PackageConstants.OutputWindow.OutputPaneName, activate);
         pane.OutputStringNoPump(message);
      }

      /// <inheritdoc />
      public void Write(string message, object arg0, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         Write(string.Format(message, arg0), activate);
      }

      /// <inheritdoc />
      public void Write(string message, object arg0, object arg1, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         Write(string.Format(message, arg0, arg1), activate);
      }

      /// <inheritdoc />
      public void Write(string message, object arg0, object arg1, object arg2, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         Write(string.Format(message, arg0, arg1, arg2), activate);
      }

      /// <inheritdoc />
      public void Write(bool predicate, string message, object arg0, object arg1, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         if (predicate)
         {
            Write(string.Format(message, arg0, arg1), activate);
         }
      }

      /// <inheritdoc />
      public void Write(bool predicate, string message, object arg0, object arg1, object arg2, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         if (predicate)
         {
            Write(string.Format(message, arg0, arg1, arg2), activate);
         }
      }

      /// <inheritdoc />
      public void WriteLine(string message, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         var pane = EnsurePane(PackageConstants.OutputWindow.OutputPaneGuid, PackageConstants.OutputWindow.OutputPaneName, activate);
         pane.OutputStringNoPump(message + "\r\n");
      }

      /// <inheritdoc />
      public void WriteLine(string message, object arg0, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         WriteLine(string.Format(message, arg0), activate);
      }

      /// <inheritdoc />
      public void WriteLine(string message, object arg0, object arg1, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         WriteLine(string.Format(message, arg0, arg1), activate);
      }

      /// <inheritdoc />
      public void WriteLine(string message, object arg0, object arg1, object arg2, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         WriteLine(string.Format(message, arg0, arg1, arg2), activate);
      }

      /// <inheritdoc />
      public void WriteLine(bool predicate, string message, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         if (predicate)
         {
            WriteLine(message, activate);
         }
      }

      /// <inheritdoc />
      public void WriteLine(bool predicate, string message, object arg0, bool activate = true)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         WriteLine(predicate, string.Format(message, arg0), activate);
      }

      private IVsOutputWindowPane EnsurePane(Guid guid, string name, bool activate)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         int result = outputWindow.GetPane(ref guid, out IVsOutputWindowPane pane);
         if (result != VSConstants.S_OK)
         {
            result = outputWindow.CreatePane(ref guid, name, 1, 1);
            _ = ErrorHandler.ThrowOnFailure(result);

            result = outputWindow.GetPane(ref guid, out pane);
            _ = ErrorHandler.ThrowOnFailure(result);
         }

         if (activate)
         {
            _ = pane.Activate();
         }
         return pane;
      }
   }
}
