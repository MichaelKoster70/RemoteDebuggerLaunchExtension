// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// The VS options page accessor service.
   /// Implements the <see cref="IOptionsPageAccessor"/> interface.
   /// </summary>
   /// <seealso cref="IOptionsPageAccessor" />
   internal class OptionsPageAccessorService : SOptionsPageAccessor,  IOptionsPageAccessor
   {
      private readonly RemoteDebuggerLauncherPackage package;

      private DeviceOptionsPage devicePage;
      private LocalOptionsPage localPage;

      /// <summary>
      /// Initializes an new instance of the <see cref="OptionsPageAccessorService"/> class.
      /// </summary>
      /// <param name="package">The package where to get the options pages from.</param>
      public OptionsPageAccessorService(RemoteDebuggerLauncherPackage package)
      {
         this.package = package;
      }

      /// <inheritdoc />
      public int QueryHostPort()
      {
         return GetDevicePage().SecureShellPort;
      }

      /// <inheritdoc />
      public string QueryUserName()
      {
         return GetDevicePage().UserName;
      }

      /// <inheritdoc />
      public string QueryPrivateKeyFilePath()
      {
         return GetDevicePage().PrivateKey;
      }

      /// <inheritdoc />
      public bool QueryForceIPv4()
      {
         return GetDevicePage().ForceIPv4;
      }

      /// <inheritdoc />
      public string QueryDotNetInstallFolderPath()
      {
         return GetDevicePage().DotNetInstallFolderPath;
      }

      /// <inheritdoc />
      public string QueryDebuggerInstallFolderPath()
      {
         return GetDevicePage().DebuggerInstallFolderPath;
      }

      /// <inheritdoc />
      public string QueryAppFolderPath()
      {
         return GetDevicePage().AppFolderPath;
      }

      /// <inheritdoc />
      public bool QueryPublishOnDeploy()
      {
         return GetLocalPage().PublishOnDeploy;
      }

      /// <inheritdoc />
      public PublishMode QueryPublishMode()
      {
         return GetLocalPage().PublishMode;
      }

      private DeviceOptionsPage GetDevicePage()
      {
         if (devicePage == null)
         {
            devicePage = package.GetDialogPage(typeof(DeviceOptionsPage)) as DeviceOptionsPage;
         }

         return devicePage;
      }

      private LocalOptionsPage GetLocalPage()
      {
         if (localPage == null)
         {
            localPage = package.GetDialogPage(typeof(LocalOptionsPage)) as LocalOptionsPage;
         }

         return localPage;
      }
   }

   /// <summary>
   /// Defines the service type for the options page accessor service.
   /// </summary>
   [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "By design, using VS naming standards")]
#pragma warning disable S101 // Types should be named in PascalCase
   internal interface SOptionsPageAccessor
   {
   }
#pragma warning restore S101 // Types should be named in PascalCase
}
