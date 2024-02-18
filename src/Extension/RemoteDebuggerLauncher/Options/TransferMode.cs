// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Mode defining the transfer protocol to be used for file transfer to the target device.
   /// </summary>
   public enum TransferMode
   {
      /// <summary>Secure Copy Protocol (SCP)</summary>
      SCP,

      /// <summary>Rsync</summary>
      Rsync
   }
}
