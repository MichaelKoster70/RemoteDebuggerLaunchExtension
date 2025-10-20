// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Text.RegularExpressions;

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
            
            // Check for traditional PEM format with encryption
            if (keyContent.Contains("ENCRYPTED") ||
                keyContent.Contains("Proc-Type: 4,ENCRYPTED") ||
                keyContent.Contains("DEK-Info:"))
            {
               return true;
            }

            // Check for OpenSSH format
            if (keyContent.Contains("BEGIN OPENSSH PRIVATE KEY"))
            {
               // Look for base64 encoded algorithm indicators
               // "aes256-ctr" = "CmFlczI1Ni1jdHI="
               // "aes128-ctr" = "CmFlczEyOC1jdHI="
               // "bcrypt" = "YmNyeXB0"
               // "none" = "BG5vbmU="
               
               if (keyContent.Contains("CmFlczI1Ni1jdHI") ||    // aes256-ctr
                   keyContent.Contains("CmFlczEyOC1jdHI") ||    // aes128-ctr
                   keyContent.Contains("CmFlczE5Mi1jdHI") ||    // aes192-ctr
                   keyContent.Contains("CmFlczI1Ni1jYmM") ||    // aes256-cbc
                   keyContent.Contains("CmFlczEyOC1jYmM") ||    // aes128-cbc
                   keyContent.Contains("CmFlczE5Mi1jYmM") ||    // aes192-cbc
                   keyContent.Contains("YmNyeXB0") ||           // bcrypt
                   keyContent.Contains("c2NyeXB0"))             // scrypt
               {
                  return true;
               }

               // If it contains "BG5vbmU=" (base64 for "none") for both cipher and kdf, it's unencrypted
               // OpenSSH format has "none" for both cipher and kdf when unencrypted
               var nonePattern = "BG5vbmU=";
               var noneOccurrences = Regex.Matches(keyContent, Regex.Escape(nonePattern)).Count;
               if (noneOccurrences >= 2)
               {
                  return false; // Both cipher and kdf are "none" - key is unencrypted
               }

               // Additional fallback checks for plaintext algorithm names
               if (keyContent.Contains("aes128-cbc") ||
                   keyContent.Contains("aes192-cbc") ||
                   keyContent.Contains("aes256-cbc") ||
                   keyContent.Contains("aes128-ctr") ||
                   keyContent.Contains("aes192-ctr") ||
                   keyContent.Contains("aes256-ctr") ||
                   keyContent.Contains("aes128-gcm") ||
                   keyContent.Contains("aes256-gcm") ||
                   keyContent.Contains("chacha20-poly1305"))
               {
                  return true;
               }
            }

            return false;
         }
         catch (Exception)
         {
            // If we can't read or parse the file, assume it's not encrypted
            return false;
         }
      }
   }
}