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
   internal static class ProjectSystemUncnfiguredProjectExtensions
   {
      public static IDebugTokenReplacer GetDebugTokenReplacerService(this UnconfiguredProject unconfiguredProject)
      {
         ThrowIf.ArgumentNull(unconfiguredProject, nameof(unconfiguredProject));

         return unconfiguredProject.Services.ExportProvider.GetService<IDebugTokenReplacer>();
      }

      public static string GetName(this UnconfiguredProject unconfiguredProject)
      {
         return Path.GetFileName(unconfiguredProject.FullPath);
      }
   }
}
