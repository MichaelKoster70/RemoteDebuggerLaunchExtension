// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class holding constant definitions.
   /// </summary>
   internal static class PackageConstants
   {
      /// <summary>
      /// Local filesystem related constants.
      /// </summary>
      public static class FileSystem
      {
         /// <summary>Directory under %localappdata% where to store data like Logs and cached assets.</summary>
         public const string StorageFolder = @"LinuxRemoteDebugger";
      }

      /// <summary>
      /// Common Project System related constants
      /// </summary>
      public static class CPS
      {
         /// <summary>
         /// Values for the <see cref="AppliesTo"/> attribute.
         /// </summary>
         public static class AppliesTo
         {
            /// <summary>CPS 'AppliesTo attribute value for the custom Launch Profile.</summary>
            public const string LaunchProfiles = "LaunchProfiles";

            /// <summary>CPS 'AppliesTo' attribute value for the custom Launch Profile.</summary>
            public const string LaunchProfilesAndCps = "LaunchProfiles + CPS";
         }
      }

      /// <summary>
      /// VS Options related constants.
      /// </summary>
      public static class Options
      {
         /// <summary>The Category name in the Tools=>Options used for this extension.</summary>
         public const string Category = "LinuxRemoteDebugger";

         /// <summary>GUID for options page holding device connectivity settings.</summary>
         public const string PageGuidDevice = "EADDD238-7A86-4B42-8BA8-66B0C318953B";

         /// <summary>GUID for options page holding device local settings.</summary>
         public const string PageGuidLocal = "F00C616B-91D8-4891-9E94-CCC7D52A1D35";

         /// <summary>The options page name holding device connectivity settings.</summary>
         public const string PageNameDevice = "Device";

         /// <summary>The options page name holding local settings.</summary>
         public const string PageNameLocal = "Local";

         /// <summary>The name for the credentials category attribute.</summary>
         public const string PageCategoryCredentials = "Credentials";

         /// <summary>The name for the folder category attribute.</summary>
         public const string PageCategoryFolders = "Folders";

         /// <summary>The name for the SSH category attribute.</summary>
         public const string PageCategorySsh = "SSH";

         /// <summary>The name for the Deployment (SCP/rsync) category attribute.</summary>
         public const string PageCategoryDeployment = "Deployment";

         /// <summary>The name for the Publish category attribute.</summary>
         public const string PageCategoryPublish = "Publish";

         /// <summary>The name for the Diagnostics category attribute.</summary>
         public const string PageCategoryDiagnostics = "Diagnostics";

         /// <summary>The default value for the SSH private key file.</summary>
         public const string DefaultValuePrivateKey = @"%userprofile%\.ssh\id_rsa";

         /// <summary>The default value for the SSH port.</summary>
         public const int DefaultValueSecureShellHostPort = 22;

         /// <summary>The default value for the Force IPv4.</summary>
         public const bool DefaultValueForceIPv4 = false;

         /// <summary>The default value for the Disable Host Key Checking.</summary>
         public const bool DefaultValueDisableHostKeyChecking = false;

         /// <summary>The default value for the .NET installation path on the device page.</summary>
         public const string DefaultValueDotNetInstallFolderPath = "~/.dotnet";

         /// <summary>The default value for the VS Code Debugger installation path on the device page.</summary>
         public const string DefaultValueDebuggerInstallFolderPath = "~/.vsdbg";

         /// <summary>The default value for the Tools installation path on the device page.</summary>
         public const string DefaultValueToolsInstallFolderPath = "~/.rdl";

         /// <summary>The default value for the App folder path on the device page.</summary>
         public const string DefaultValueAppFolderPath = "~/$(MSBuildProjectName)";
      }

      /// <summary>
      /// .NET related constants.
      /// </summary>
      public static class Dotnet
      {
         /// <summary>The name of the 'dotnet' binary.</summary>
         public const string BinaryName = "dotnet";

         /// <summary>The URL of the install script to install .NET manually on a Linux system.</summary>
         public const string GetInstallDotnetShUrl = "https://dot.net/v1/dotnet-install.sh";

         /// <summary>The URL of the install script to install .NET manually on a windows system.</summary>
         public const string GetInstallDotnetPs1Url = "https://dot.net/v1/dotnet-install.ps1";

         /// <summary>Directory under %localappdata% where to cache the .NET downloads.</summary>
         public const string DownloadCacheFolder = FileSystem.StorageFolder + @"\dotnet";
      }

      /// <summary>
      /// Launch Profile related constants.
      /// </summary>
      public static class LaunchProfile
      {
         /// <summary>The command name for the 'SecureShellRemoteLaunchProfile' Launch Profile.</summary>
         public const string CommandName = "SshRemoteLaunch";
      }

      public static class DebugLaunchSettings
      {
         /// <summary>The Engine GUID for the VS Code managed debugger.</summary>
         public static readonly Guid EngineGuid = new Guid("541B8A8A-6081-4506-9F0A-1CE771DEBC04");

         /// <summary>Name the connection adapter when using Windows SSH.</summary>
         public const string AdapterNameWindowsSSH = "ssh.exe";
      }

      public static class OutputWindow
      {
         /// <summary>The GUID for the custom output pane.</summary>
         public static readonly Guid OutputPaneGuid = new Guid("772F56E5-2D88-40FB-9006-50B9C72A2A97");

         /// <summary>The name for the custom output pane.</summary>
         public const string OutputPaneName = "Linux Remote Debugger";
      }

      public static class Debugger
      {
         /// <summary>The name of the VS debugger binary.</summary>
         public const string BinaryName = "vsdbg";
         public const string GetVsDbgShUrl = "https://aka.ms/getvsdbgsh";
         public const string GetVsDbgPs1Url = "https://aka.ms/getvsdbgps1";

         /// <summary>Directory under %localappdata% where to cache the remote debugger downloads.</summary>
         public const string DownloadCacheFolder = FileSystem.StorageFolder + @"\vsdbg\vs2022";
      }

      public static class Commands
      {
         /// <summary>Command menu group (command set GUID).</summary>
         public static readonly Guid CommandSet = new Guid("67dde3fd-abea-469b-939f-02a3178c91e7");
      }

      public static class Publish
      {
         /// <summary>Publish output relative to project base output dir.</summary>
         public const string OutDir = "Deploy";
      }

      public static class SecureShell
      {
         /// <summary>Filename of the default SSH private key.</summary>
         public const string DefaultPrivateKeyFileName = "id_rsa";

         /// <summary>Filename of the default SSH public key.</summary>
         public const string DefaultPublicKeyFileName = "id_rsa.pub";

         /// <summary>Filename of the default ECDSA SSH private key.</summary>
         public const string DefaultPrivateKeyFileNameEcdsa = "id_ecdsa";

         /// <summary>Filename of the default ECDSA SSH public key.</summary>
         public const string DefaultPublicKeyFileNameEcdsa = "id_ecdsa.pub";

         /// <summary>Folder relative to user home where the SSH key are stored.</summary>
         public const string DefaultKeyPairFolder = ".ssh";

         /// <summary>SSH key generator executable.</summary>
         public const string KeyGenExecutable = "ssh-keygen.exe";

         /// <summary>SSH key generator arguments for RSA keys (4096 bit).</summary>
         public const string KeyGenArgumentsRsa = "-b 4096 -t rsa -f {0} -q -N \"\" -m pem";

         /// <summary>SSH key generator arguments for ECDSA keys (256 bit curve).</summary>
         public const string KeyGenArgumentsEcdsa = "-t ecdsa -b 256 -f {0} -q -N \"\" -m pem";

         /// <summary>SSH key scanner executable.</summary>
         public const string KeyScanExecutable = "ssh-keyscan.exe";

         /// <summary>Filename of the known_hosts file.</summary>
         public const string DefaultKnownHostsFileName = "known_hosts";

         /// <summary>SSH key scanner arguments.</summary>
         /// <remarks>No space after {2} because SshForceIPv4 includes a trailing space.</remarks>
         public const string KeyScanArguments = "{2}-p {1} {0}";

         /// <summary>Host Fingerprint scanner: SSH executable.</summary>
         public const string HostFingerPrintSshExecutable = "ssh.exe";

         /// <summary>The SSH arguments to add server fingerprint to known_hosts file.</summary>
         /// <remarks>No space after {4} because SshForceIPv4 includes a trailing space.</remarks>
         public const string HostFingerPrintSshArguments = "{0}@{1} {4}-p {2} -i \"{3}\" \"echo DONE\"";

         /// <summary>HTTPS Developer Certificate name.</summary>
         public const string HttpsCertificateName = "DevCert.pfx";
      }

      /// <summary>
      /// Holds command line options for the Windows OpenSSH client.
      /// </summary>
      public static class SecureShellOption
      {
         private const string PrivateKeyOption = "-i \"{0}\" ";
         private const string PortOption = "-p {0} ";

         /// <summary>SSH option to force IPv4 usage => -4</summary>
         public const string ForceIPv4 = "-4 ";

         /// <summary>SSH option disable host key checking (and to not use known_hosts file) => -o StrictHostKeyChecking.</summary>
         public const string DisableHostKeyChecking = "-o StrictHostKeyChecking=no -o UserKnownHostsFile=NUL -o GlobalKnownHostsFile=NUL ";

         /// <summary>Formats the SSH private key option => -i {0}</summary>
         /// <param name="privateKeyFilePath">The private key file path.</param>
        public static string FormatPrivateKeyOption(string privateKeyFilePath) => string.Format(PrivateKeyOption, privateKeyFilePath);

         /// <summary>Formats the SSH port option => -p {0}</summary>
         public static string FormatPortOption(int port) => string.Format(PortOption, port);
      }

      /// <summary>
      /// Holder for all Linux Shell commands executed via SSH.
      /// </summary>
      public static class LinuxShellCommands
      {
         private const string MkDir = "mkdir -p \"{0}\"";
         private const string Rm = "rm {0}";
         private const string RmF = "rm -f {0}";
         private const string RmRf = "rm -rf {0}";
         private const string Chmod = "chmod {0} {1}";
         private const string ChmodPlusX = "chmod +x {0}";
         private const string Command = "command -v {0}";

         /// <summary>"pwd" - Command to get the current working directory.</summary>
         public const string Pwd = "pwd";

         /// <summary>"mkdir -p \"{0}\"" - Formats the CreateDirectory command with the specified path.</summary>
         public static string FormatMkDir(string path) => string.Format(MkDir, path);

         /// <summary>"rm {0}" - Formats the RemoveFile command with the specified path.</summary>
         public static string FormatRm(string path) => string.Format(Rm, path);

         /// <summary>"rm -f {0}" - Formats the ForceRemoveFile command with the specified path.</summary>
         public static string FormatRmF(string path) => string.Format(RmF, path);

         /// <summary>"rm -rf {0}" - Formats the RemoveDirectory command with the specified path.</summary>
         public static string FormatRmRf(string path) => string.Format(RmRf, path);

         /// <summary>"chmod {0} {1}" - Formats the chmod command with the specified mode and path.</summary>
         public static string FormatChmod(string mode, string path) => string.Format(Chmod, mode, path);

         /// <summary>"chmod +x {0}" - Formats the chmod command with the specified path.</summary>
         public static string FormatChmodPlusX(string path) => string.Format(ChmodPlusX, path);

         /// <summary>"command -v {0}" - Formats the command to check if a command exists on the remote system.</summary>
         public static string FormatCommand(string commandName) => string.Format(Command, commandName);
      }
   }
}
