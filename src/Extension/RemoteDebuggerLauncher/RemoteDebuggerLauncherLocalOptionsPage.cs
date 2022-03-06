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
   internal class RemoteDebuggerLauncherLocalOptionsPage : DialogPage
   {
      [Category("Local Settings")]
      [DisplayName("PuTTY Installation Path")]
      [Description("The path where PuTTY is installed.")]
      [DefaultValue("")]
      public string PuttyInstallPath { get; set; } = String.Empty;
   }
}
