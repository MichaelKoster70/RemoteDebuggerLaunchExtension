// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   internal static class LaunchProfileExtensions
   {
      public static bool IsSecureShellRemoteLaunch(this ILaunchProfile profile) => profile.CommandName.Equals(PackageConstants.LaunchProfile.CommandName, StringComparison.Ordinal);
   }
}
