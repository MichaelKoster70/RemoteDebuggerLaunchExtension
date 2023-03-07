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
   /// Wait Dialog Factory service implementation creating dialog instances.
   /// Implements the <see cref="IWaitDialogFactoryService" />
   /// </summary>
   /// <seealso cref="IWaitDialogFactoryService" />
   internal class WaitDialogFactoryService : IWaitDialogFactoryService
   {
      private readonly IVsThreadedWaitDialogFactory factory;

      /// <summary>
      /// Initializes a new instance of the <see cref="WaitDialogFactoryService"/> class.
      /// </summary>
      public WaitDialogFactoryService()
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         factory = Package.GetGlobalService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
      }

      /// <inheritdoc />
      public IWaitDialogService Create(string waitCaption, string waitMessage, string progressText, int delayToShowDialog = 1, string statusbarText = null, StatusbarAnimation? statusbarAnimation = null)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         // create the dialog instance
         var vsWaitDialogInstance = factory.CreateInstance();

         var instance = new WaitDialogService(vsWaitDialogInstance);
         instance.Start(waitCaption, waitMessage, progressText, delayToShowDialog, statusbarText, statusbarAnimation);

         return instance;
      }
   }
}
