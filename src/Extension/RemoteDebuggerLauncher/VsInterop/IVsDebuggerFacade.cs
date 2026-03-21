// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

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
