// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface providing events for solution changes.
   /// </summary>
   internal interface IVsSolutionEventsFacade
   {
      /// <summary>
      /// Occurs when when Visual Studio after VS closed a solution.
      /// </summary>
      event EventHandler AfterCloseSolution;
   }
}
