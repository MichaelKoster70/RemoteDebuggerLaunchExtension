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
      /// Common Project System relalated constants
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
         }
      }

      /// <summary>
      /// VS Options related constants.
      /// </summary>
      public static class Options
      {
         /// <summary>The Category name in the Tools=>Options used for this extension.</summary>
         public const string Category = "RemoteDebuggerLauncher";

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

         /// <summary>The name for the Publish category attribute.</summary>
         public const string PageCategoryPublish = "Publish";

         /// <summary>The default value for the SSH private key file.</summary>
         public const string DefaultValuePrivateKey = @"%userprofile%\.ssh\id_rsa";

         /// <summary>The default value for the SSH port.</summary>
         public const int DefaultValueSecureShellHostPort = 22;

         /// <summary>The default value for the .NET installation path on the device page.</summary>
         public const string DefaultValueDotNetInstallFolderPath = "~/.dotnet";

         /// <summary>The default value for the VS Code Debugger installation path on the device page.</summary>
         public const string DefaultValueDebuggerInstallFolderPath = "~/.vsdbg";

         /// <summary>The default value for the App folder path on the device page.</summary>
         public const string DefaultValueAppFolderPath = "~/project";
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
         public const string DownloadCacheFolder = @"RemoteDebuggerLauncher\dotnet";
      }

      /// <summary>
      /// Launch Profile related constants.
      /// </summary>
      public static class LaunchProfile
      {
         /// <summary>The command name for the 'SecureShellRemoteLaunchProfile' Launch Profile.</summary>
         public const string CommandName = "SecureShellRemoteLaunch";
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
         public const string OutputPaneName = "Remote Debugger";
      }

      public static class Debugger
      {
         /// <summary>The name of the VS debugger binary.</summary>
         public const string BinaryName = "vsdbg";
         public const string GetVsDbgShUrl = "https://aka.ms/getvsdbgsh";
         public const string GetVsDbgPs1Url = "https://aka.ms/getvsdbgps1";

         /// <summary>Directory under %localappdata% where to cache the remote debugger downloads.</summary>
         public const string DownloadCacheFolder = @"RemoteDebuggerLauncher\vsdbg\vs2022";
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
   }
}
