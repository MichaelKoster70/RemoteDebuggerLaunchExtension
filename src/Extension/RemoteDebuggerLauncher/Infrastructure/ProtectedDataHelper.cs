// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Security.Cryptography;
using System.Text;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class providing string encryption support using Windows DPAPI
   /// </summary>
   internal static class ProtectedDataHelper
   {
      public static string Protect(string value)
      {
         var valueBytes = Encoding.Unicode.GetBytes(value);
         var encryptedBytes = ProtectedData.Protect(valueBytes, null, DataProtectionScope.CurrentUser);
         return Convert.ToBase64String(encryptedBytes);
      }

      public static string Unprotect(string encryptedValue)
      {
         var encryptedBytes = Convert.FromBase64String(encryptedValue);
         var valueBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
         return Encoding.Unicode.GetString(valueBytes);
      }
   }
}
