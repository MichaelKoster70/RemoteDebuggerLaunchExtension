// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.PlatformUI;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Modal Dialog providing the UI options for the Setup HTTPS Command.
   /// Implements the <see cref="DialogWindow" />
   /// </summary>
   /// <seealso cref="DialogWindow" />
   public partial class SetupHttpsDialogWindow : DialogWindow
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="SetupHttpsDialogWindow"/> class.
      /// </summary>
      public SetupHttpsDialogWindow()
      {
         InitializeComponent();
      }
   }
}
