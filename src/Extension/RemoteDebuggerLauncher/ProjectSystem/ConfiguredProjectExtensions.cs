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
   /// <summary>
   /// Extension methods for the Project System <see cref="ConfiguredProject"/> class.
   /// </summary>
   internal static class ConfiguredProjectExtensions
   {
      /// <summary>
      /// Gets the absolute path from the MSBUILD property 'OutDir'.
      /// </summary>
      /// <param name="configuredProject">The CPS configured project to read from.</param>
      /// <returns>The string holding the absolute path.</returns>
      public static async Task<string> GetOutputDirectoryPathAsync(this ConfiguredProject configuredProject)
      {
         var projectFolder = Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);
         var outputDir = await configuredProject.GetOutputDirectoryAsync();

         return Path.Combine(projectFolder, outputDir);
      }

      /// <summary>
      /// Gets the absolute path from the MSBUILD property 'BaseOutputPath'.
      /// </summary>
      /// <param name="configuredProject">The CPS configured project to read from.</param>
      /// <returns>The string holding the absolute path.</returns>
      public static async Task<string> GetBaseOutputDirectoryPathAsync(this ConfiguredProject configuredProject)
      {
         var projectFolder = Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);
         var outputDir = await configuredProject.GetBaseOutputDirectoryAsync();

         return Path.Combine(projectFolder, outputDir);
      }

      /// <summary>
      /// Gets the relative output directory from the MSBUILD property 'OutDir'.
      /// </summary>
      /// <param name="configuredProject">The CPS configured project to read from.</param>
      /// <returns>The string holding the directory name.</returns>
      public static Task<string> GetOutputDirectoryAsync(this ConfiguredProject configuredProject)
      {
         var projectPropertiesProvider = configuredProject.Services.ProjectPropertiesProvider;
         ThrowIf.NotPresent(projectPropertiesProvider);

         return projectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync("OutDir");
      }

      /// <summary>
      /// Gets the relative output directory from the MSBUILD property 'BaseOutputPath'.
      /// </summary>
      /// <param name="configuredProject">The CPS configured project to read from.</param>
      /// <returns>The string holding the directory name.</returns>
      public static Task<string> GetBaseOutputDirectoryAsync(this ConfiguredProject configuredProject)
      {
         var projectPropertiesProvider = configuredProject.Services.ProjectPropertiesProvider;
         ThrowIf.NotPresent(projectPropertiesProvider);

         return projectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync("BaseOutputPath");
      }

      /// <summary>
      /// Gets the relative output directory from the MSBUILD property 'BaseOutputPath'.
      /// </summary>
      /// <param name="configuredProject">The CPS configured project to read from.</param>
      /// <returns>The string holding the directory name.</returns>
      public static Task<string> GetTargetFrameworkAsync(this ConfiguredProject configuredProject)
      {
         var projectPropertiesProvider = configuredProject.Services.ProjectPropertiesProvider;
         ThrowIf.NotPresent(projectPropertiesProvider);

         return projectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync("TargetFramework");
      }

      /// <summary>
      /// Gets the assembly file name from the MSBUILD property 'AssemblyName'.
      /// </summary>
      /// <param name="configuredProject">The CPS configured project to read from.</param>
      /// <returns>The string holding the assembly name.</returns>
      public static Task<string> GetAssemblyNameAsync(this ConfiguredProject configuredProject)
      {
         var projectPropertiesProvider = configuredProject.Services.ProjectPropertiesProvider;
         ThrowIf.NotPresent(projectPropertiesProvider);

         return projectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync("AssemblyName");
      }

      /// <summary>
      /// Gets the assembly DLL file name.
      /// </summary>
      /// <param name="configuredProject">The CPS configured project to read from.</param>
      /// <returns>The string holding the assembly DLL name.</returns>
      public async static Task<string> GetAssemblyFileNameAsync(this ConfiguredProject configuredProject)
      {
         var assemblyName = await GetAssemblyNameAsync(configuredProject);

         return $"{assemblyName}.dll";
      }

      /// <summary>
      /// Gets the the project name.
      /// </summary>
      /// <param name="configuredProject">The CPS configured project to read from.</param>
      /// <returns>The <see langword="string"/> holding the name.</returns>
      public static string GetProjectName(this ConfiguredProject configuredProject) => Path.GetFileNameWithoutExtension(configuredProject.UnconfiguredProject.FullPath);

      /// <summary>
      /// Gets the the project folder.
      /// </summary>
      /// <param name="configuredProject">The CPS configured project to read from.</param>
      /// <returns>The <see langword="string"/> holding the folder.</returns>
      public static string GetProjectFolder(this ConfiguredProject configuredProject) => Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);

   }
}
