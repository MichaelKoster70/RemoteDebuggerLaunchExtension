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
      /// VS Options related constants.
      /// </summary>
      public static class Options
      {
         /// <summary>The Category name in the Tools=>Options used for this extension.</summary>
         public const string Category = "Remote Debugger Launcher";

         /// <summary>The options page name holding device connectivity settings.</summary>
         public const string PageDevice = "Device";

         /// <summary>The options page name holding local settings.</summary>
         public const string PageLocal = "Local";

         /// <summary>The default value for the SSH private key file.</summary>
         public const string DefaultValuePrivateKey = @"%userprofile%\.ssh\id_rsa";

         /// <summary>The default value for the .NET installation path on the device page.</summary>
         public const string DefaultValueDotNetInstallFolderPath = "~/.dotnet";

         /// <summary>The default value for the VS Code Debugger installation path on the device page.</summary>
         public const string DefaultValueDebuggerInstallFolderPath = "~/.vsdbg";

         /// <summary>The default value for the App folder path on the device page.</summary>
         public const string DefaultValueAppFolderPath = "~/project";
      }

      /// <summary>
      /// VS Settings related constants.
      /// </summary>
      public static class Settings
      {
         /// <summary>The settings section name.</summary>
         public const string Name = "Remote Debugger Settings";
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

         /// <summary>CPS 'AppliesTo attribute value for the custom Launch Profile.</summary>
         public const string AppliesTo = "LaunchProfiles";
      }

      public static class DebugLaunchSettings
      {
         /// <summary>The Engine GUID for the VS Code managed debugger.</summary>
         public static readonly Guid EngineGuid = new Guid("541B8A8A-6081-4506-9F0A-1CE771DEBC04");

         public static class Options
         {
            /// <summary>Name the connection adapter when using Windows SSH.</summary>
            public const string AdapterNameWindowsSSH = "ssh.exe";
         }
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
   }
}
