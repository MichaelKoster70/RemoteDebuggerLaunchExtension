// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining the VS options page accessor service.
   /// </summary>
   internal interface IOptionsPageAccessor
   {
      string QueryUserName();

      string QueryPrivateKeyFilePath();

      AdapterProviderKind QueryAdapterProvider();

      string QueryDotNetInstallFolderPath();

      string QueryDebuggerInstallFolderPath();

      string QueryPuttyInstallPath();

      string QueryAppFolderPath();
   }

   /// <summary>
   /// Defines the service type for the options page accessor service.
   /// </summary>
   internal interface SOptionsPageAccessor
   {
   }
}