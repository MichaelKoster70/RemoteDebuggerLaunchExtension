// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Service for managing SSH private key passphrases securely.
   /// </summary>
   internal class SecureShellPassphraseService : ISecureShellPassphraseService
   {
      private readonly ConcurrentDictionary<string, SecureString> passphraseCache = new();

      /// <inheritdoc/>
      public string GetCachedPassphrase(string privateKeyFilePath)
      {
         if (string.IsNullOrEmpty(privateKeyFilePath))
            return null;

         var normalizedPath = Path.GetFullPath(privateKeyFilePath);
         if (passphraseCache.TryGetValue(normalizedPath, out var securePassphrase))
         {
            return ConvertToUnsecureString(securePassphrase);
         }

         return null;
      }

      /// <inheritdoc/>
      public async Task<string> PromptAndCachePassphraseAsync(string privateKeyFilePath)
      {
         if (string.IsNullOrEmpty(privateKeyFilePath))
            return null;

         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         var dialog = new SecureShellPassphraseDialog();
         var keyFileName = Path.GetFileName(privateKeyFilePath);
         dialog.SetKeyFile(keyFileName);

         var result = dialog.ShowDialog();
         if (result == true && !string.IsNullOrEmpty(dialog.Passphrase))
         {
            var normalizedPath = Path.GetFullPath(privateKeyFilePath);
            var securePassphrase = ConvertToSecureString(dialog.Passphrase);
            passphraseCache.AddOrUpdate(normalizedPath, securePassphrase, (key, oldValue) => securePassphrase);
            
            return dialog.Passphrase;
         }

         return null;
      }

      /// <inheritdoc/>
      public void ClearCache()
      {
         foreach (var kvp in passphraseCache)
         {
            kvp.Value?.Dispose();
         }
         passphraseCache.Clear();
      }

      private static SecureString ConvertToSecureString(string password)
      {
         if (password == null)
            return null;

         var securePassword = new SecureString();
         foreach (char c in password)
         {
            securePassword.AppendChar(c);
         }
         securePassword.MakeReadOnly();
         return securePassword;
      }

      private static string ConvertToUnsecureString(SecureString securePassword)
      {
         if (securePassword == null)
            return null;

         IntPtr unmanagedString = IntPtr.Zero;
         try
         {
            unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(securePassword);
            return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString);
         }
         finally
         {
            System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
         }
      }
   }
}