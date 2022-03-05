// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   internal class RemoteDebuggerLauncherDeviceOptionsPage : DialogPage
   {
      [Category("Remote Device")]
      [DisplayName("User Name")]
      [Description("The default user name to be used for connecting to a target device.")]
      public string UserName { get; set; } = "user";

      [Category("Remote Device")]
      [DisplayName("Private Key")]
      [Description("The default private key to be used for connecting to the target device.")]
      public string PrivateKey { get; set; } = String.Empty;
   }
}
