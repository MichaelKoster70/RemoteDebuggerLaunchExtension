// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Security.Cryptography;
using System.Text;

namespace RemoteDebuggerLauncher.WebTools
{
   internal static class PasswordGenerator
   {
      public static string Generate(int length = 12)
      {
         const string Valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
         StringBuilder res = new StringBuilder();
         using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
         {
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (length-- > 0)
            {
               rng.GetBytes(uintBuffer);
               uint num = BitConverter.ToUInt32(uintBuffer, 0);
               res.Append(Valid[(int)(num % (uint)Valid.Length)]);
            }
         }
         return res.ToString();
      }
   }
}
