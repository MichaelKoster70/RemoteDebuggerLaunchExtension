// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Implements the Device Options page shown in the VS options tree under "Remote Debugger Launcher"
   /// </summary>
   [ComVisible(true)]
   [Guid(PackageConstants.Options.PageGuidDevice)]
   public class DeviceOptionsPage : DialogPage
   {
      private int secureShellPort = PackageConstants.Options.DefaultValueSecureShellHostPort;

      [Category(PackageConstants.Options.PageCategoryCredentials)]
      [DisplayName("User name")]
      [Description("The default user name to be used for connecting to a target device.")]
      public string UserName { get; set; } = "user";

      [Category(PackageConstants.Options.PageCategoryCredentials)]
      [DisplayName("Private key")]
      [Description("The default private key to be used for connecting to the target device.")]
      [DefaultValue(PackageConstants.Options.DefaultValuePrivateKey)]
      public string PrivateKey { get; set; } = PackageConstants.Options.DefaultValuePrivateKey;

      [Category(PackageConstants.Options.PageCategorySsh)]
      [DisplayName("Host port")]
      [Description("The default SSH port number on the target device.")]
      [DefaultValue(PackageConstants.Options.DefaultValueSecureShellHostPort)]
      public int SecureShellPort
      {
         get => secureShellPort;
         set
         {
            // make sure the entered port stays withing the correct boundaries
            if (IPEndPoint.MinPort > value || value > IPEndPoint.MaxPort)
            {
               throw new ArgumentOutOfRangeException(nameof(SecureShellPort), string.Format(ExceptionMessages.InvalidSecureShellPortValue, IPEndPoint.MinPort, IPEndPoint.MaxPort));
            }
            secureShellPort = value;
         }
      }

      [Category(PackageConstants.Options.PageCategorySsh)]
      [DisplayName("Force IPv4")]
      [Description("To force the plugin to use IPv4.")]
      [DefaultValue(PackageConstants.Options.DefaultValueForceIPv4)]
      public bool ForceIPv4 { get; set; } = PackageConstants.Options.DefaultValueForceIPv4;

      [Category(PackageConstants.Options.PageCategoryTransfer)]
      [DisplayName("Transfer Mode")]
      [Description("The means to copy assets to the target device.")]
      [DefaultValue(TransferMode.SecureCopyFull)]
      public TransferMode TransferMode { get; set; } = TransferMode.SecureCopyFull;

      [Category(PackageConstants.Options.PageCategoryFolders)]
      [DisplayName(".NET install folder path")]
      [Description("The folder path where .NET framework is installed on the target device.")]
      [DefaultValue(PackageConstants.Options.DefaultValueDotNetInstallFolderPath)]
      public string DotNetInstallFolderPath { get; set; } = PackageConstants.Options.DefaultValueDotNetInstallFolderPath;

      [Category(PackageConstants.Options.PageCategoryFolders)]
      [DisplayName("VS Code debugger install folder path")]
      [Description("The folder path where the VS code debugger is installed on the target device.")]
      [DefaultValue(PackageConstants.Options.DefaultValueDebuggerInstallFolderPath)]
      public string DebuggerInstallFolderPath { get; set; } = PackageConstants.Options.DefaultValueDebuggerInstallFolderPath;

      [Category(PackageConstants.Options.PageCategoryFolders)]
      [DisplayName("App folder path")]
      [Description("The path on the target device where the application binaries will get deployed to.")]
      [DefaultValue(PackageConstants.Options.DefaultValueAppFolderPath)]
      public string AppFolderPath { get; set; } = PackageConstants.Options.DefaultValueAppFolderPath;

      protected override void OnActivate(CancelEventArgs e)
      {
         ResetInvalidPortNumberToDefault();
         base.OnActivate(e);
      }

      protected override void OnApply(PageApplyEventArgs e)
      {
         ResetInvalidPortNumberToDefault();
         base.OnApply(e);
      }

      private void ResetInvalidPortNumberToDefault()
      {
         // make sure the entered port stays withing the correct boundaries
         if (IPEndPoint.MinPort > secureShellPort || secureShellPort > IPEndPoint.MaxPort)
         {
            secureShellPort = PackageConstants.Options.DefaultValueSecureShellHostPort;
         }
      }
   }
}
