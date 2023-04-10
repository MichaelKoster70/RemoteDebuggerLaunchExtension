// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher.SecureShell
{
   /// <summary>
   /// Interface for the high level operations performed on the remote device.
   /// </summary>
   internal interface ISecureShellRemoteOperationsService
   {
      /// <summary>
      /// Gets or sets a value indicating whether the Host name should be logged to the output pane.
      /// </summary>
      /// <value><c>true</c> to append to log; otherwise, <c>false</c>.</value>
      bool LogHost { get; set; }

      /// <summary>
      /// Checks whether a connection with the remove device can be established.
      /// </summary>
      /// <param name="logProgress"><c>true</c>  to log progress, else <c>false</c>.</param>
      /// <returns>A Task representing the asynchronous operation.</returns>
      /// <exception cref="RemoteDebuggerLauncherException">Thrown when connection cannot be established.</exception>
      Task CheckConnectionThrowAsync(bool logProgress = true);

      /// <summary>
      /// Queries the user home directory on the remote device.
      /// </summary>
      /// <returns>A <see cref="Task{string}"/> representing the asynchronous operation: the user home directory; null if command fails.</returns>
      Task<string> QueryUserHomeDirectoryAsync();

      /// <summary>
      /// Tries to install the VS Code assuming the target device has a direct internet connection.
      /// Removes the previous installation if present.
      /// </summary>
      /// <param name="version">The version to install.</param>
      /// <returns>A <see cref="Task{Boolean}"/> representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      /// <remarks>See https://docs.microsoft.com/en-us/dotnet/iot/debugging?tabs=self-contained&pivots=vscode</remarks>
      Task<bool> TryInstallVsDbgOnlineAsync(string version = Constants.Debugger.VersionLatest);

      /// <summary>
      /// Tries to install the VS Code assuming the target device has no internet connection.
      /// Removes the previous installation if present.
      /// </summary>
      /// <returns>A <see cref="Task"/>representing the asynchronous operation.</returns>
      /// <remarks>
      /// The downloaded vsdbg version gets cached under %localappdata%\RemoteDebuggerLauncher\vsdbg\vs2022
      /// </remarks>
      Task TryInstallVsDbgOfflineAsync(string version = Constants.Debugger.VersionLatest);

      /// <summary>
      /// Deploys the application to the remote device.
      /// </summary>
      /// <param name="sourcePath">The absolute path to the source directory to deploy.</param>
      /// <param name="clean"><c>true</c> to clean the target folder; else <c>false</c>.</param>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <remarks>The target path on the remote device is read from the launch profile setting.</remarks>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task DeployRemoteFolderAsync(string sourcePath, bool clean);

      /// <summary>
      /// Cleans the application folder on the remote device.
      /// </summary>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <remarks>The target path on the remote device is read from the launch profile setting.</remarks>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task CleanRemoteFolderAsync();

      /// <summary>
      /// Changes the remote file permission to the given value
      /// </summary>
      /// <param name="remotePath">The absolute path of the file to change.</param>
      /// <param name="permission">The file permission.</param>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task ChangeRemoteFilePermissionAsync(string remotePath, int permission);

      /// <summary>
      /// Tries to install the .NET assuming the target device has a direct internet connection.
      /// </summary>
      /// <param name="channel">The channel source holding the version to install.</param>
      /// <returns>A <see cref="Task{Boolean}" />representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      /// <exception cref="NotSupportedException">runtime kind not supported.</exception>
      /// <remarks>See Microsoft Docs: https://docs.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual</remarks>
      Task<bool> TryInstallDotNetOnlineAsync(DotnetInstallationKind kind, string channel);

      /// <summary>
      /// Tries to install .NET assuming the target device has no internet connection.
      /// </summary>
      /// <param name="kind">The kind of installation to perform.</param>
      /// <param name="channel">The channel source holding the version to install.</param>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <exception cref="NotSupportedException">runtime kind not supported.</exception>
      Task TryInstallDotNetOfflineAsync(DotnetInstallationKind kind, string channel);

      /// <summary>
      /// Get the CPU architecture to determine which runtime ID to use, ignoring MacOS and Alpine based Linux when determining the needed runtime ID.
      /// </summary>
      /// <returns>The <see cref="string"/> hoilding the runtime ID.</returns>
     Task<string> GetRuntimeIdAsync();
   }
}