// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Composition;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Service for managing SSH private key passphrases securely.
   /// </summary>
   [Export(typeof(ISecureShellKeyPassphraseService)), Shared]

   internal class SecureShellKeyPassphraseService : ISecureShellKeyPassphraseService
   {
      private readonly ConcurrentDictionary<string, byte[]> passphraseCache = new ConcurrentDictionary<string, byte[]>();

      /// <inheritdoc/>
      public bool TryGet(string privateKeyFilePath, out string passphrase)
      {
         passphrase = null;

         if (string.IsNullOrEmpty(privateKeyFilePath))
         {
            return false;
         }

         var normalizedPath = Path.GetFullPath(privateKeyFilePath);
         if (passphraseCache.TryGetValue(normalizedPath, out var encryptedPassphrase))
         {
            passphrase = DecryptPassphrase(encryptedPassphrase);
            return passphrase != null;
         }

         return false;
      }

      /// <inheritdoc/>
      public bool Prompt(string privateKeyFilePath, out string passphrase)
      {
         passphrase = null;

         ThreadHelper.ThrowIfNotOnUIThread();

         if (string.IsNullOrEmpty(privateKeyFilePath))
         {
            return false;
         }

         (var viewModel,var result) = DialogFactory.CreateAndShowDialog<SecureShellPassphraseDialogWindow, SecureShellPassphraseViewModel>(
            new SecureShellPassphraseViewModel(ThreadHelper.JoinableTaskFactory, privateKeyFilePath));

         if (result.HasValue && result.Value && !string.IsNullOrEmpty(viewModel.Passphrase))
         {
            var normalizedPath = Path.GetFullPath(privateKeyFilePath);
            var encryptedPassphrase = EncryptPassphrase(viewModel.Passphrase);
            _ = passphraseCache.AddOrUpdate(normalizedPath, encryptedPassphrase, (key, oldValue) => encryptedPassphrase);

            passphrase = viewModel.Passphrase;
            return true;
         }
         else
         {
            // User canceled or did not enter a passphrase
            return false;
         }
      }

      /// <inheritdoc/>
      public void Clear()
      {
         passphraseCache.Clear();
      }

      /// <inheritdoc/>
      public void Clear(string privateKeyFilePath)
      {
         if (!string.IsNullOrEmpty(privateKeyFilePath))
         {
            var normalizedPath = Path.GetFullPath(privateKeyFilePath);
            _= passphraseCache.TryRemove(normalizedPath, out _);
         }
      }

      private static byte[] EncryptPassphrase(string passphrase)
      {
         if (string.IsNullOrEmpty(passphrase))
         {
            // Handle empty passphrase
            return Array.Empty<byte>();
         }

         var plainBytes = System.Text.Encoding.UTF8.GetBytes(passphrase);
         return ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
      }

      private static string DecryptPassphrase(byte[] encryptedPassphrase)
      {
         if (encryptedPassphrase == null || encryptedPassphrase.Length == 0)
         {
            // Handle empty passphrase
            return null;
         }

         var plainBytes = ProtectedData.Unprotect(encryptedPassphrase, null, DataProtectionScope.CurrentUser);
         return System.Text.Encoding.UTF8.GetString(plainBytes);
      }
   }
}