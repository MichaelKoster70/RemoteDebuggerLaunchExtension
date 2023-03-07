// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Wait Dialog Service implementation providing the wait dialogs.
   /// Implements the <see cref="IWaitDialogService" />
   /// </summary>
   /// <seealso cref="IWaitDialogService" />
   internal class WaitDialogService : IWaitDialogService
   {
      private IVsThreadedWaitDialog4 instance;

      /// <summary>
      /// Initializes a new instance of the <see cref="WaitDialogService"/> class.
      /// </summary>
      /// <param name="instance">The VS dialog instance to manage.</param>
      internal WaitDialogService(IVsThreadedWaitDialog4 instance)
      {
         this.instance = instance;
      }

      /// <inheritdoc />
      public void Update(string waitMessage, string progressText, string statusbarText)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         instance.UpdateProgress(waitMessage, progressText, statusbarText, 0, 0, true, out _);
      }

      /// <summary>
      /// Starts the.wait dialog.
      /// </summary>
      /// <param name="waitCaption">The wait dialog caption.</param>
      /// <param name="waitMessage">The wait message.</param>
      /// <param name="progressText">The progress text.</param>
      /// <param name="delayToShowDialog">The number of seconds to delay showing the dialog.</param>
      /// <param name="statusbarText">The optional status bar text.</param>
      /// <param name="statusbarAnimation">The optional status animation.</param>
      public void Start(string waitCaption, string waitMessage, string progressText, int delayToShowDialog = 1, string statusbarText = null, StatusbarAnimation? statusbarAnimation = null)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         object varStatusBmpAnimation = statusbarAnimation.HasValue ? (short)statusbarAnimation : (object)null;
         instance.StartWaitDialog(waitCaption, waitMessage, progressText, varStatusBmpAnimation, statusbarText, delayToShowDialog, false, true);
      }

      /// <inheritdoc />
      public void Dispose()
      {
         _ = instance?.EndWaitDialog();
         instance = null;
      }
   }
}
