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
      /// <summary>The Category name in the Tools=>Options used for this extension.</summary>
      public const string OptionsCategory = "Remote Debugger Launcher";

      /// <summary>The options page name holding device connectivity settings.</summary>
      public const string OptionsPageDevice = "Device";

      /// <summary>The options page name holding local settings.</summary>
      public const string OptionsPageLocal = "Local";

      /// <summary>The option name on the device page for the user name.</summary>
      public const string OptionsNameUserName = "UserName";

      /// <summary>The option name on the device page for the private key.</summary>
      public const string OptionsNamePrivateKey = "PrivateKey";

      /// <summary>The option name on the local page for the PuTTY installation Path.</summary>
      public const string OptionsPuttyInstallPath = "PuttyInstallPath";

      /// <summary>CPS 'AppliesTo attribute value for the custom Launch Profile.</summary>
      public const string AppliesToLaunchProfiles = "LaunchProfiles";

      /// <summary>Name the connection adapter when using Windows SSH.</summary>
      public const string AdapterNameWindowsSSH = "ssh.exe";

      /// <summary>The command name for the 'SecureShellRemoteLaunchProfile' Launch Profile Name.</summary>
      public const string SecureShellRemoteLaunchProfileName = "SecureShellRemoteLaunchProfile";

      /// <summary>The command name for the 'SecureShellRemoteLaunchProfile' Launch Profile.</summary>
      public const string SecureShellRemoteLaunchCommandName = "SecureShellRemoteLaunch";

      /// <summary>The Engine GUID for the VS Code managed debugger.</summary>
      public static readonly Guid EngineGuid = new Guid("541B8A8A-6081-4506-9F0A-1CE771DEBC04");
   }
}
