using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace RemoteDebuggerLauncher
{
   internal static class ProjectSystemConfiguredProjectExtensions
   {
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
