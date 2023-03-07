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
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;

namespace RemoteDebuggerLauncher
{
   internal class VsDebuggerFacade : IVsDebuggerFacade
   {
      private readonly Lazy<IVsDebugger> debugger;
      private readonly IServiceProvider serviceProvider;

      [ImportingConstructor]
      public VsDebuggerFacade(IServiceProvider serviceProvider)
      {
         this.serviceProvider = serviceProvider;

         debugger = new Lazy<IVsDebugger>(() => this.serviceProvider.GetService<IVsDebugger, IVsDebugger>());
      }

      public bool IsJavaScriptDebuggingOnLaunchEnabled()
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         ((IVsDebugger7)debugger.Value).IsJavaScriptDebuggingOnLaunchEnabled(out bool debuggingEnabled);
         return debuggingEnabled;
      }

      public bool NotifyBeforeLaunchWithoutJavaScriptDebugger()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         ((IVsDebugger9)debugger.Value).NotifyBeforeLaunchWithoutJavaScriptDebugger(out int enabled);
         return enabled == 1;
      }
   }
}
