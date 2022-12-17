﻿// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace RemoteDebuggerLauncher
{
   internal class OptionsPageAccessorService : SOptionsPageAccessor,  IOptionsPageAccessor
   {
      private readonly RemoteDebuggerLauncherPackage package;

      private DeviceOptionsPage devicePage;
      private LocalOptionsPage localPage;

      public OptionsPageAccessorService(RemoteDebuggerLauncherPackage package)
      {
         this.package = package;
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
   internal interface SOptionsPageAccessor
   {
   }
}