// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Extension methods for the Project System <see cref="UnconfiguredProject"/> class.
   /// </summary>
   internal static class UnconfiguredProjectExtensions
   {
      public static string GetProjectFolder(this UnconfiguredProject project) => Path.GetDirectoryName(project.FullPath);
   }
}
