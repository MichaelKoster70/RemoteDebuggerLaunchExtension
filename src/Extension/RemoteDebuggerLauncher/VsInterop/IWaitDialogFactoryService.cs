// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining a factory for wait dialogs.
   /// </summary>
   internal interface IWaitDialogFactoryService
   {
      /// <summary>
      /// Create a new instance of a <see cref="IWaitDialogService"/> service.
      /// </summary>
      /// <param name="waitCaption">The wait dialog caption.</param>
      /// <param name="waitMessage">The wait message.</param>
      /// <param name="progressText">The progress text.</param>
      /// <param name="delayToShowDialog">The number of seconds to delay showing the dialog.</param>
      /// <param name="statusbarText">The optional status bar text.</param>
      /// <param name="statusbarAnimation">The optional status animation.</param>
      /// <returns>The configured and started instance.</returns>
      IWaitDialogService Create(string waitCaption, string waitMessage, string progressText, int delayToShowDialog = 1, string statusbarText = null, StatusbarAnimation? statusbarAnimation = null);
   }
}
