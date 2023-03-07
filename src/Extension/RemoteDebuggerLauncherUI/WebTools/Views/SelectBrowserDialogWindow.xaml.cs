// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.PlatformUI;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Modal Dialog providing the UI options for the Install .NET Command.
   /// Implements the <see cref="DialogWindow" />
   /// </summary>
   /// <seealso cref="DialogWindow" />
   public partial class SelectBrowserDialogWindow : DialogWindow
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="SelectBrowserDialogWindow"/> class.
      /// </summary>
      public SelectBrowserDialogWindow()
      {
         InitializeComponent();
      }
   }
}

