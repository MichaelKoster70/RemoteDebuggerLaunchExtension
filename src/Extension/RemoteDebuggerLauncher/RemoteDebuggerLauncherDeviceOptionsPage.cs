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
      [DisplayName("User name")]
      [Description("The default user name to be used for connecting to a target device.")]
      public string UserName { get; set; } = "user";

      [Category("Remote Device")]
      [DisplayName("Private key")]
      [Description("The default private key to be used for connecting to the target device.")]
      public string PrivateKey { get; set; } = String.Empty;

      [Category("Remote Device")]
      [DisplayName(".NET install folder path")]
      [Description("The folder path where .NET framework is installed on the target device.")]
      [DefaultValue(PackageConstants.Options.DefaultValueDotNetInstallFolderPath)]
      public string DotNetInstallFolderPath { get; set; } = PackageConstants.Options.DefaultValueDotNetInstallFolderPath;

      [Category("Remote Device")]
      [DisplayName("VS Code debugger install folder path")]
      [Description("The folder path where the VS code debugger is installed on the target device.")]
      [DefaultValue(PackageConstants.Options.DefaultValueDebuggerInstallFolderPath)]
      public string DebuggerInstallFolderPath { get; set; } = PackageConstants.Options.DefaultValueDebuggerInstallFolderPath;

      [Category("Remote Device")]
      [DisplayName("App folder path")]
      [Description("The path on the target device where the application binaries will get deployed to.")]
      [DefaultValue(PackageConstants.Options.DefaultValueAppFolderPath)]
      public string AppFolderPath { get; set; } = PackageConstants.Options.DefaultValueAppFolderPath;
   }
}
