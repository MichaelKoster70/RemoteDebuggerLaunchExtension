// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>The supported SSH authentication kinds</summary>
   internal enum AuthenticationKind
   {
      /// <summary>Password based authentication.</summary>
      Password,

      /// <summary>Private key based authentication.</summary>
      PrivateKey,
   }
}
