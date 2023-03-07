// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   internal static class DebugLaunchOptionsExtensions
   {
      internal static bool IsDebugging(this DebugLaunchOptions launchOptions) => !launchOptions.HasFlag(DebugLaunchOptions.NoDebug);
   }
}
