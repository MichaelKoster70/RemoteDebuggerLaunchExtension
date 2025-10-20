// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Dialog for entering SSH private key passphrase.
   /// </summary>
   internal partial class SecureShellPassphraseDialog : Window
   {
      public SecureShellPassphraseDialog()
      {
         InitializeComponent();
         PassphraseBox.Focus();
      }

      /// <summary>
      /// Gets the entered passphrase.
      /// </summary>
      public string Passphrase { get; private set; }

      /// <summary>
      /// Sets the key file name in the dialog.
      /// </summary>
      /// <param name="keyFileName">The name of the key file.</param>
      public void SetKeyFile(string keyFileName)
      {
         KeyFileLabel.Content = $"Enter passphrase for private key: {keyFileName}";
      }

      private void OkButton_Click(object sender, RoutedEventArgs e)
      {
         Passphrase = PassphraseBox.Password;
         DialogResult = true;
         Close();
      }

      private void CancelButton_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = false;
         Close();
      }

      private void PassphraseBox_PasswordChanged(object sender, RoutedEventArgs e)
      {
         OkButton.IsEnabled = !string.IsNullOrEmpty(PassphraseBox.Password);
      }
   }
}