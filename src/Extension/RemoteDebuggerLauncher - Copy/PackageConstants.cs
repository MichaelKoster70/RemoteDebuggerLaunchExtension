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
      /// <summary>CPS 'AppliesTo attribute value for the custom Launch Profile.</summary>
      public const string AppliesToLaunchProfiles = "LaunchProfiles";

      /// <summary>The command name for the 'SecureShellRemoteLaunchProfile' Launch Profile Name.</summary>
      public const string SecureShellRemoteLaunchProfileName = "SecureShellRemoteLaunchProfile";

      /// <summary>The command name for the 'SecureShellRemoteLaunchProfile' Launch Profile.</summary>
      public const string SecureShellRemoteLaunchCommandName = "SecureShellRemoteLaunch";

      /// <summary>The Engine GUID for the VS Code managed debugger.</summary>
      public static readonly Guid EngineGuid = new Guid("541B8A8A-6081-4506-9F0A-1CE771DEBC04");
   }
}
