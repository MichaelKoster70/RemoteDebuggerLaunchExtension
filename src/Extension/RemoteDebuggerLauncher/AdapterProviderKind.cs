// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------
namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Defines the supported connection adapter kind.
   /// </summary>
   internal enum AdapterProviderKind
   {
      /// <summary>The built-in Windows SSH.</summary>
      WindowsSSH,

      /// <summary>PuTTY</summary>
      PuTTY
   }
}
