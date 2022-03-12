// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace RemoteDebuggerLauncher
{
   internal static class ProjectSystemConfiguredProjectExtensions
   {
      public static async Task<string> GetOutputDirectoryPathAsync(this ConfiguredProject configuredProject)
      {
         var projectFolder = Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);
         var outputDir = await configuredProject.GetOutputDirectoryAsync();

         return Path.Combine(projectFolder, outputDir);
      }

      public static Task<string> GetOutputDirectoryAsync(this ConfiguredProject configuredProject)
      {
         var projectPropertiesProvider = configuredProject.Services.ProjectPropertiesProvider;
         ThrowIf.NotPresent(projectPropertiesProvider);

         return projectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync("OutDir");
      }

      public static Task<string> GetAssemblyNameAsync(this ConfiguredProject configuredProject)
      {
         var projectPropertiesProvider = configuredProject.Services.ProjectPropertiesProvider;
         ThrowIf.NotPresent(projectPropertiesProvider);

         return projectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync("AssemblyName");
      }

      public async static Task<string> GetAssemblyFileNameAsync(this ConfiguredProject configuredProject)
      {
         var assemblyName = await GetAssemblyNameAsync(configuredProject);

         return $"{assemblyName}.dll";
      }
   }
}
