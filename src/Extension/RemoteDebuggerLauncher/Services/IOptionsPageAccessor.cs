// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining the VS options page accessor service.
   /// </summary>
   internal interface IOptionsPageAccessor
   {
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
   }


   /// <summary>
   /// Defines the service type for the options page accessor service.
   /// </summary>
   [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "By design, using VS naming standards")]
   internal interface SOptionsPageAccessor
   {
   }
}