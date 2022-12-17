﻿// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Statusbar Service implementation writing to a the Visual Studio Statusbar.
   /// Implements the <see cref="RemoteDebuggerLauncher.IStatusbarService" />
   /// </summary>
   /// <seealso cref="RemoteDebuggerLauncher.IStatusbarService" />
   internal class StatusbarService : SStatusbarService, IStatusbarService
   {
      private readonly IVsStatusbar statusbar;

      /// <summary>
      /// Initializes a new instance of the <see cref="StatusbarService"/> class.
      /// </summary>
      public StatusbarService()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         statusbar = Package.GetGlobalService(typeof(SVsStatusbar)) as IVsStatusbar;
      }

      /// <inheritdoc />
      public void SetText(string text)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         _ = statusbar.SetText(text);
      }

      /// <inheritdoc />
      public void SetText(string text, object arg0)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         _ = statusbar.SetText(string.Format(text, arg0));
      }

      public void Clear()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         _ = statusbar.Clear();
      }
   }

   /// <summary>
   /// Defines the service type for the status bar service.
   /// </summary>
   [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "By design, using VS naming standards")]
   internal interface SStatusbarService
   {
   }
}