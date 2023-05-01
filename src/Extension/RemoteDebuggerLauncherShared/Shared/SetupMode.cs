// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.Shared
{
   /// <summary>
   /// The HTTPS certificate setup mode
   /// </summary>
   public enum SetupMode
   {
      /// <summary>Update the certificate if invalid or expired.</summary>
      Update,

      /// <summary>Replace the existing certificate with a new one.</summary>
      Replace
   }
}
