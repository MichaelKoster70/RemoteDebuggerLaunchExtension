// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Mode defining the transfer protocol to be used for file transfer to the target device.
   /// </summary>
   [TypeConverter(typeof(TransferModeDisplayNameConverter))]
   public enum TransferMode
   {
      /// <summary>Use Secure Copy Protocol (SCP), send all files.</summary>
      [Display(Name = "SCP, All Files")]
      SecureCopyFull,

      /// <summary>Use Secure Copy Protocol (SCP), send only changed files.</summary>
      [Display(Name = "SCP, Changed Files")]
      SecureCopyDelta,

      /// <summary>Use Rsync</summary>
      [Display(Name = "Rsync")]
      Rsync
   }
}
