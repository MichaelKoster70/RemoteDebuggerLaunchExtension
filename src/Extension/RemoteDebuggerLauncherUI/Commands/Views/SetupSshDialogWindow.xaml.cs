// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
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

      // Event handler for Create button click to show dropdown menu
      private void CreateKeyButton_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         if (sender is System.Windows.Controls.Button button && button.ContextMenu != null)
         {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
         }
      }

      // Called via XAML Binding UpdateSourceExceptionFilter
      public object HostPortUpdateSourceExceptionFilter(object _, Exception exception)
      {
         if (DataContext is SetupSshViewModel vm && exception != null)
         {
            // Push the error into the VM so OkCommand can reflect it
            vm.ReportBindingException("HostPort", exception.Message);
         }

         // Return the exception to keep WPF validation behavior and tooltip
         return exception;
      }
   }
}
