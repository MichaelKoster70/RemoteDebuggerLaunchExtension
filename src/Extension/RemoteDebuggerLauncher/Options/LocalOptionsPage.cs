// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   internal class LocalOptionsPage : DialogPage
   {
      [Category("Local Settings")]
      [DisplayName("Adapter provider")]
      [Description("The remote connectivity provider.")]
      public AdapterProviderKind AdapterProvider { get; set; } = AdapterProviderKind.WindowsSSH;

      [Category("Local Settings")]
      [DisplayName("PuTTY installation path")]
      [Description("The path where PuTTY is installed.")]
      [DefaultValue("")]
      public string PuttyInstallPath { get; set; } = String.Empty;
   }
}
