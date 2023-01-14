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
      /// <summary>
      /// Queries the port to be used to establish a connection to the remote device from the device option page.
      /// </summary>
      /// <returns>A <see langword="int"/> holding the port, <c>PackageConstants.Options.DefaultValueSecureShellHostPort</c> if not configured.</returns>
      int QueryHostPort();

      /// <summary>
      /// Queries the user name to be used to establish a connection to the remote device from the device option page.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the name, <c>string.Empty</c> if not configured.</returns>
      string QueryUserName();

      /// <summary>
      /// Queries the private key to be used to establish a connection to the remote device from the device option page.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the name, <c>string.Empty</c> if not configured.</returns>
      string QueryPrivateKeyFilePath();

      /// <summary>
      /// Queries the folder path where the .NET framework is installed on the remote device from the device option page.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the path, <c>string.Empty</c> if not configured.</returns>
      string QueryDotNetInstallFolderPath();

      /// <summary>
      /// Queries the path where the VS Code Debugger is installed on the remote device from the device option page.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the path, <c>string.Empty</c> if not configured.</returns>
      string QueryDebuggerInstallFolderPath();

      /// <summary>
      /// Queries the path where the application gets deployed on the remote device from the device option page.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the path, <c>string.Empty</c> if not configured.</returns>
      string QueryAppFolderPath();

      /// <summary>
      /// Queries the flag whether to publish the application on deploy.
      /// </summary>
      /// <returns>A <see langword="bool"/><c>true</c> to deploy published output;<c>false</c> to deploy build output.</returns>
      bool QueryPublishOnDeploy();
   }
}