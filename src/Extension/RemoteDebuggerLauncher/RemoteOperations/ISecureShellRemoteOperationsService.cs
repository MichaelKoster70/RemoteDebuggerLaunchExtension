// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher.RemoteOperations
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
      /// Deploys the application binaries to the remote device.
      /// </summary>
      /// <param name="sourcePath">The absolute path to the source directory to deploy.</param>
      /// <param name="clean"><c>true</c> to clean the target folder; else <c>false</c>.</param>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <remarks>The target path on the remote device is read from the launch profile setting.</remarks>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task DeployRemoteFolderAsync(string sourcePath, bool clean);

      /// <summary>
      /// Deploys a folder to a specific target path on the remote device.
      /// </summary>
      /// <param name="sourcePath">The absolute path to the source directory to deploy.</param>
      /// <param name="remoteTargetPath">The absolute remote target path where the folder should be deployed.</param>
      /// <param name="clean"><c>true</c> to clean the target folder; else <c>false</c>.</param>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task DeployRemoteFolderAsync(string sourcePath, string remoteTargetPath, bool clean);

      /// <summary>
      /// Deploys a single file to the remote device.
      /// </summary>
      /// <param name="sourceFilePath">The absolute path to the source file to deploy.</param>
      /// <param name="remoteTargetPath">The absolute remote target path where the file should be deployed.</param>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task DeployRemoteFileAsync(string sourceFilePath, string remoteTargetPath);

      /// <summary>
      /// Cleans the application folder on the remote device.
      /// </summary>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <remarks>The target path on the remote device is read from the launch profile setting.</remarks>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task CleanRemoteFolderAsync();

      /// <summary>
      /// Changes the remote file permission to the given value numeric permission set
      /// </summary>
      /// <param name="remotePath">The absolute path of the file to change.</param>
      /// <param name="permissions">The permissions (user/group/others) in numeric form.</param>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task ChangeRemoteFilePermissionAsync(string remotePath, int permissions);

      /// <summary>
      /// Changes the remote file permission to the given value string representation (rwx)
      /// </summary>
      /// <param name="remotePath">The absolute path of the file to change.</param>
      /// <param name="permissions">The permission (user/group/others) in string form (rwx).</param>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <exception cref="SecureShellSessionException">thrown when the operation failed.</exception>
      Task ChangeRemoteFilePermissionAsync(string remotePath, string permissions);

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
      /// Tries the find the path where .NET is installed on the remote device.
      /// </summary>
      /// <returns>A <see cref="Task{String}" />representing the asynchronous operation: the path where .NET is installed, or <c>null</c> if not found.</returns>
      Task<string> TryFindDotNetInstallPathAsync();

      /// <summary>
      /// Sets up a new ASP.NET HTTPS Developer Certificate.
      /// </summary>
      /// <param name="mode">The desired setup mode.</param>
      /// <param name="certificate">The x.509 certificate in PFX form.</param>
      /// <param name="password">The password to unlock the PFX content.</param>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      Task SetupAspNetDeveloperCertificateAsync(SetupMode mode, byte[] certificate, string password);

      /// <summary>
      /// Get the CPU architecture to determine which runtime ID to use, ignoring MacOS and Alpine based Linux when determining the needed runtime ID.
      /// </summary>
      /// <returns>The <see cref="string"/> holding the runtime ID.</returns>
      Task<string> GetRuntimeIdAsync();

      /// <summary>
      /// Queries environment variables from a running process owned by the current user.
      /// </summary>
      /// <param name="processName">The name of the process to query.</param>
      /// <returns>A <see cref="Task{Dictionary}"/> representing the asynchronous operation: a dictionary of environment variables, or an empty dictionary if the process is not found.</returns>
      Task<Dictionary<string, string>> QueryProcessEnvironmentAsync(string processName);
   }
}