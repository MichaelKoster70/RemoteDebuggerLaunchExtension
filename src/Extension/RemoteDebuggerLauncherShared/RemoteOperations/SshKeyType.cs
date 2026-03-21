// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Enumeration of supported SSH key types.
   /// </summary>
   public enum SshKeyType
   {
      /// <summary>
      /// RSA key with 4096 bit length.
      /// </summary>
      Rsa,

      /// <summary>
      /// ECDSA key with 256 bit curve.
      /// </summary>
      Ecdsa
   }
}
