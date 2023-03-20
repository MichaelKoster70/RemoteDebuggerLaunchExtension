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
   /// Modal Dialog providing the UI options for the Setup SSH Command.
   /// Implements the <see cref="DialogWindow" />
   /// </summary>
   /// <seealso cref="DialogWindow" />
   public partial class SetupSshDialogWindow : DialogWindow, IPasswordProvider
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="SetupSshDialogWindow"/> class.
      /// </summary>
      public SetupSshDialogWindow()
      {
         InitializeComponent();
      }

      public void QueryPassword(out string password)
      {
         password = passwordBox.Password;
      }
   }
}
