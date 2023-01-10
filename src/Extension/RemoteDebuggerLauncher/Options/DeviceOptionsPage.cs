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
#pragma warning disable CA1812 // By design, Visual Studio will instanciate the class for us
   /// <summary>
   /// Implements the Device Options page shown in the VS options tree under "Remote Debugger Launcher"
   /// </summary>
   internal class DeviceOptionsPage : DialogPage
   {
      [Category("Credentials")]
      [DisplayName("User name")]
      [Description("The default user name to be used for connecting to a target device.")]
      public string UserName { get; set; } = "user";

      [Category("Credentials")]
      [DisplayName("Private key")]
      [Description("The default private key to be used for connecting to the target device.")]
      [DefaultValue(PackageConstants.Options.DefaultValuePrivateKey)]
      public string PrivateKey { get; set; } = PackageConstants.Options.DefaultValuePrivateKey;

      [Category("SSH")]
      [DisplayName("Port")]
      [Description("The default SSH port number on the target device.")]
      [DefaultValue(PackageConstants.Options.DefaultValueSecureShellPort)]
      public int SecureShellPort { get; set; } = PackageConstants.Options.DefaultValueSecureShellPort;

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
#pragma warning restore CA1812

}
