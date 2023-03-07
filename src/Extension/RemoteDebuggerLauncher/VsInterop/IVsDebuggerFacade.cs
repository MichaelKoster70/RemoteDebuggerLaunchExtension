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

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Facade for the Visual Studio Debugger.
   /// </summary>
   /// <remarks>exposed as MEF component.</remarks>
   internal interface IVsDebuggerFacade
   {
      bool IsJavaScriptDebuggingOnLaunchEnabled();

      bool NotifyBeforeLaunchWithoutJavaScriptDebugger();
   }
}
