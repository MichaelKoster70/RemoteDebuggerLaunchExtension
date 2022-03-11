// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   internal class OptionsPageAccessorService : SOptionsPageAccessor,  IOptionsPageAccessor
   {
      private readonly RemoteDebuggerLauncherPackage package;

      private RemoteDebuggerLauncherDeviceOptionsPage devicePage;
      private RemoteDebuggerLauncherLocalOptionsPage localPage;

      public OptionsPageAccessorService(RemoteDebuggerLauncherPackage package)
      {
         this.package = package;
      }

      public AdapterProviderKind QueryAdapterProvider()
      {
         return GetLocalPage().AdapterProvider;
      }

      public string QueryAppFolderPath()
      {
         return GetDevicePage().AppFolderPath;
      }

      public string QueryDebuggerInstallFolderPath()
      {
         return GetDevicePage().DebuggerInstallFolderPath;
      }

      public string QueryDotNetInstallFolderPath()
      {
         return GetDevicePage().DotNetInstallFolderPath;
      }

      public string QueryPrivateKeyFilePath()
      {
         return GetDevicePage().PrivateKey;
      }

      public string QueryPuttyInstallPath()
      {
         return GetLocalPage().PuttyInstallPath;
      }

      public string QueryUserName()
      {
         return GetDevicePage().UserName;
      }

      private RemoteDebuggerLauncherDeviceOptionsPage GetDevicePage()
      {
         if (devicePage == null)
         {
            devicePage = package.GetDialogPage(typeof(RemoteDebuggerLauncherDeviceOptionsPage)) as RemoteDebuggerLauncherDeviceOptionsPage;
         }

         return devicePage;
      }

      private RemoteDebuggerLauncherLocalOptionsPage GetLocalPage()
      {
         if (localPage == null)
         {
            localPage = package.GetDialogPage(typeof(RemoteDebuggerLauncherLocalOptionsPage)) as RemoteDebuggerLauncherLocalOptionsPage;
         }

         return localPage;
      }
   }
}
