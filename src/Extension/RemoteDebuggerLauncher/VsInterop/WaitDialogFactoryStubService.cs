// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Wait Dialog Factory service implementation creating dialog stub instances.
   /// Implements the <see cref="IWaitDialogFactoryService" />
   /// </summary>
   /// <seealso cref="IWaitDialogFactoryService" />
   internal class WaitDialogFactoryStubService : IWaitDialogFactoryService
   {
#pragma warning disable S3881 // "IDisposable" should be implemented correctly - not needed as we have nothing to dispose
      internal class WaitDialogService : IWaitDialogService
      {
         public void Dispose()
         {
            //EMPTY_BODY
         }

         public void Update(string waitMessage, string progressText, string statusbarText)
         {
            //EMPTY_BODY
         }
      }
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
      /// <summary>
      /// Initializes a new instance of the <see cref="WaitDialogFactoryStubService"/> class.
      /// </summary>
      public WaitDialogFactoryStubService()
      {
      }

      /// <inheritdoc />
      public IWaitDialogService Create(string waitCaption, string waitMessage, string progressText, int delayToShowDialog = 1, string statusbarText = null, StatusbarAnimation? statusbarAnimation = null)
      {
         return new WaitDialogService();
      }
   }
}
