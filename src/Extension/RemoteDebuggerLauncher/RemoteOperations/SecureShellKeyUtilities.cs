// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Utility class for SSH private key operations.
   /// </summary>
   internal static class SecureShellKeyUtilities
   {
      /// <summary>
      /// Determines if a private key file is encrypted (requires a passphrase).
      /// </summary>
      /// <param name="privateKeyFilePath">The path to the private key file.</param>
      /// <returns>True if the key is encrypted; false otherwise.</returns>
      public static bool IsPrivateKeyEncrypted(string privateKeyFilePath)
      {
         if (string.IsNullOrEmpty(privateKeyFilePath) || !File.Exists(privateKeyFilePath))
         {
            return false;
         }

         try
         {
            var keyContent = File.ReadAllText(privateKeyFilePath);
            
            // Check for common encryption indicators in PEM format
            return keyContent.Contains("ENCRYPTED") ||
                   keyContent.Contains("Proc-Type: 4,ENCRYPTED") ||
                   keyContent.Contains("DEK-Info:") ||
                   (keyContent.Contains("BEGIN OPENSSH PRIVATE KEY") && keyContent.Contains("aes"));
         }
         catch (Exception)
         {
            // If we can't read the file, assume it's not encrypted
            return false;
         }
      }
   }
}