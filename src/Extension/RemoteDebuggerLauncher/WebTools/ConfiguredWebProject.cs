// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using System.IO;
using Microsoft.VisualStudio.ProjectSystem;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Provides information for Web projects on configured level.
   /// Implements <see cref="IConfiguredWebProject"/>
   /// </summary>
   [Export(typeof(IConfiguredWebProject))]
   [AppliesTo("DotNetCoreWeb")]
   internal class ConfiguredWebProject : IConfiguredWebProject
   {
      private readonly ConfiguredProject configuredProject;

      [ImportingConstructor]
      public ConfiguredWebProject(ConfiguredProject configuredProject, IStaticWebAssetsCollectorService staticWebAssets)
      {
         this.configuredProject = configuredProject;
         StaticWebAssets= staticWebAssets;
      }

      /// <inheritdoc/>
      public string WebRoot => configuredProject.Services.ThreadingPolicy.ExecuteSynchronously(async () =>
      {
         string webRootRelative = await configuredProject.Services.ProjectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync("WebRootFolder");
         webRootRelative = string.IsNullOrWhiteSpace(webRootRelative) ? "wwwroot" : webRootRelative;

         try
         {
            string webRootAbsolute = Path.GetFullPath(Path.Combine(configuredProject.UnconfiguredProject.GetProjectFolder(), webRootRelative));
            if (Directory.Exists(webRootAbsolute))
            {
               return webRootAbsolute;
            }
         }
         catch (Exception)
         {
            // in case of an exception, ignore the configured value, use the project folder
         }

         return configuredProject.GetProjectFolder();
      });

      /// <inheritdoc/>
      public string ProjectName => configuredProject.GetProjectName();

      /// <inheritdoc/>
      public IStaticWebAssetsCollectorService StaticWebAssets { get; }
   }
}
