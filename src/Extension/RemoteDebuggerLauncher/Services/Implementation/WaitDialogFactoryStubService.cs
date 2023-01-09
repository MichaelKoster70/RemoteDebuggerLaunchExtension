// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Wait Dialog Factory service implementation creating dialog instances.
   /// Implements the <see cref="IWaitDialogFactoryService" />
   /// </summary>
   /// <seealso cref="IWaitDialogFactoryService" />
   internal class WaitDialogFactoryStubService : SWaitDialogFactoryStubService, IWaitDialogFactoryService
   {
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

   /// <summary>
   /// Defines the service type for the wait dialog factory service.
   /// </summary>
   [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "By design, using VS naming standards")]
   internal interface SWaitDialogFactoryStubService
   {
   }

}
