// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.RightsManagement;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Wait Dialog Factory service implementation creating dialog stub instances.
   /// Implements the <see cref="IWaitDialogFactoryService" />
   /// </summary>
   /// <seealso cref="IWaitDialogFactoryService" />
   internal class WaitDialogFactoryStubService : IWaitDialogFactoryService
   {
      private readonly IStatusbarService statusbar;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly - not needed as we have nothing to dispose
      internal class WaitDialogService : IWaitDialogService
      {
         private readonly IStatusbarService statusbar;

         public WaitDialogService(IStatusbarService statusbar)
         {
            this.statusbar = statusbar;
         }

         public void Dispose()
         {
            //EMPTY_BODY
         }

         public void Update(string waitMessage, string progressText, string statusbarText)
         {
            if (!string.IsNullOrEmpty(statusbarText))
            {
               statusbar.SetText(statusbarText);
            }
         }
      }
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
      /// <summary>
      /// Initializes a new instance of the <see cref="WaitDialogFactoryStubService"/> class.
      /// </summary>
      public WaitDialogFactoryStubService(IStatusbarService statusbar)
      {
         this.statusbar = statusbar;
      }

      /// <inheritdoc />
      public IWaitDialogService Create(string waitCaption, string waitMessage, string progressText, int delayToShowDialog = 1, string statusbarText = null, StatusbarAnimation? statusbarAnimation = null)
      {
         var dialog = new WaitDialogService(statusbar);

         dialog.Update(null, null, statusbarText);

         return dialog;
      }
   }
}
