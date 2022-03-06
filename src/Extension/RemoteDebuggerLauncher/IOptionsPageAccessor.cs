// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   internal interface IOptionsPageAccessor
   {
      string QueryUserName();

      string QueryPrivateKeyFilePath();

      AdapterProviderKind QueryAdapterProvider();

      string QueryDotNetInstallPath();

      string QueryDebuggerInstallPath();

      string QueryPuttyInstallPath();

      string QueryAppFolderPath();
   }
}