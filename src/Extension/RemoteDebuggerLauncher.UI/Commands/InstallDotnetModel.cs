// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.PlatformUI;

namespace RemoteDebuggerLauncher
{
   public class InstallDotnetModel
   {
      public InstallDotnetModel()
      {
      }

      public bool Online { get; set; } = true;
   }
}