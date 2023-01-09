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
   /// Interface defining a service showing a wait dialog and writing to the VS status bar.
   /// </summary>
   internal interface IWaitDialogService : IDisposable
   {
      /// <summary>
      /// Sets the status bar text in the text area.
      /// </summary>
      /// <param name="waitMessage">The updated wait message. Can be <c>null</c>.</param>
      /// <param name="progressText">The progress text. Can be <c>null</c>.</param>
      /// <param name="statusbarText">The optional status bar text. Can be <c>null</c>.</param>
      void Update(string waitMessage, string progressText, string statusbarText);
   }
}
