// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Newtonsoft.Json;

namespace RemoteDebuggerLauncher.WebTools
{
   [Export(typeof(IStaticWebAssetsCollectorService))]
   internal class StaticWebAssetsCollectorService : IStaticWebAssetsCollectorService
   {
      public const string WWWRootPathEnding = "\\wwwroot\\";
      public const string StaticAssetsObjFileName = "staticwebassets.development.json";
      public const string ContentPrefix = "_content/";

      private readonly ConfiguredProject configuredProject;
      private IReadOnlyList<StaticWebAsset> assetFileContentRoots;
      private Dictionary<string, string> assetFileFilteredRoots;

      [ImportingConstructor]
      public StaticWebAssetsCollectorService(ConfiguredProject configuredProject)
      {
         this.configuredProject = configuredProject;
      }

      public async Task<IReadOnlyDictionary<string, string>> GetPathMappingsAsync()
      {
         await LoadStaticAssetsAsync();
         return assetFileFilteredRoots;
      }

      public async Task<IReadOnlyList<StaticWebAsset>> GetAllStaticAssetsAsync()
      {
         await LoadStaticAssetsAsync();
         return assetFileContentRoots;
      }

      private async Task LoadStaticAssetsAsync()
      {
         string staticAssetsFile = await GetStaticAssetsFileAsync();
         if (staticAssetsFile != null)
         {
            try
            {
               assetFileContentRoots = ProcessStaticAssetsFile(staticAssetsFile);
               assetFileFilteredRoots = assetFileContentRoots != null ? GetFilteredAssets(assetFileContentRoots) : null;
            }
            catch (Exception)
            {
               //ignore any exceptions
            }
         }
      }

      private static Dictionary<string, string> GetFilteredAssets(IReadOnlyList<StaticWebAsset> allContentRoots)
      {
        var filteredAssets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
         foreach (StaticWebAsset allContentRoot in (IEnumerable<StaticWebAsset>)allContentRoots)
         {
            var key = $"/{allContentRoot.RelativeUrl}";
            if (allContentRoot.RelativeUrl.StartsWith(ContentPrefix, StringComparison.OrdinalIgnoreCase) && (allContentRoot.Path.EndsWith(WWWRootPathEnding, StringComparison.OrdinalIgnoreCase) || !filteredAssets.TryGetValue(key, out string _)))
            {
               filteredAssets[key] = allContentRoot.Path;
            }
         }
         return filteredAssets;
      }

      private async Task<string> GetStaticAssetsFileAsync()
      {
         var properties = configuredProject.Services.ProjectPropertiesProvider.GetCommonProperties();
         string frameworkMoniker = await properties.GetEvaluatedPropertyValueAsync("TargetFrameworkMoniker");
         if (!string.IsNullOrEmpty(frameworkMoniker))
         {
            var framework = new FrameworkName(frameworkMoniker);
            if (framework.IsNET60OrNewer())
            {
               string outputPath = await properties.GetEvaluatedPropertyValueAsync("IntermediateOutputPath");

               if (!string.IsNullOrEmpty(outputPath))
               {
                  if (!Path.IsPathRooted(outputPath))
                  {
                     outputPath = Path.Combine(configuredProject.UnconfiguredProject.GetProjectFolder(), outputPath);
                  }

                  outputPath = Path.Combine(outputPath, StaticAssetsObjFileName);
                  return File.Exists(outputPath) ? outputPath : null;
               }
            }
         }

         return null;
      }

      private IReadOnlyList<StaticWebAsset> ProcessStaticAssetsFile(string staticAssetsFilename)
      {
         var staticAssetList = new List<StaticWebAsset>();

         var jsonText = File.ReadAllText(staticAssetsFilename);
         var staticWebAssets = JsonConvert.DeserializeObject<JsonStaticWebAssets>(jsonText);
         if ((staticWebAssets?.ContentRoots) != null && staticWebAssets.ContentRoots.Count != 0 && (staticWebAssets.Root?.Children) != null)
         {
            foreach (KeyValuePair<string, ChildAsset> child in staticWebAssets.Root.Children)
            {
               ProcessStaticAssetChild(child.Key, child.Value, string.Empty, staticWebAssets.ContentRoots, staticAssetList);
            }
         }

         return staticAssetList;
      }

      private void ProcessStaticAssetChild(string childName, ChildAsset childNode, string currentUrl, List<string> contentRoots, List<StaticWebAsset> staticAssetList)
      {
         currentUrl = currentUrl.Length == 0 ? childName : currentUrl + "/" + childName;
         if (childNode.Children == null)
         {
            if (childNode.Asset?.SubPath == null || childNode.Asset.ContentRootIndex < 0 || childNode.Asset.ContentRootIndex >= contentRoots.Count)
            {
               return;
            }

            int length = currentUrl.IndexOf(childNode.Asset.SubPath, StringComparison.OrdinalIgnoreCase);
            if (length == -1)
            {
               return;
            }
            AddStaticWebAsset(currentUrl.Substring(0, length), contentRoots[childNode.Asset.ContentRootIndex]);
         }
         else
         {
            foreach (var child in childNode.Children)
            {
               ProcessStaticAssetChild(child.Key, child.Value, currentUrl, contentRoots, staticAssetList);
            }
         }

         void AddStaticWebAsset(string baseUrl, string path)
         {
            int length = baseUrl.Length;
            if (length == 0)
            {
               baseUrl = "/";
            }
            else if (length > 1)
            {
               baseUrl = baseUrl.TrimEnd('/');
            }

            foreach (var contentRoot in staticAssetList)
            {
               if (string.Equals(contentRoot.RelativeUrl, baseUrl, StringComparison.OrdinalIgnoreCase) && string.Equals(contentRoot.Path, path, StringComparison.OrdinalIgnoreCase))
               {
                  return;
               }
            }

            staticAssetList.Add(new StaticWebAsset(baseUrl, path));
         }
      }

      #region private class JsonStaticWebAssets
#pragma warning disable CA1812, S3459, S1144 // false positive: will be instanciated by JSON deserializer
      private sealed class JsonStaticWebAssets
      {
         public List<string> ContentRoots { get; set; }

         public ChildAsset Root { get; set; }
      }

      private sealed class ChildAsset
      {
         public Dictionary<string, ChildAsset> Children { get; set; }

         public AssetInfo Asset { get; set; }
      }

      private sealed class AssetInfo
      {
         public int ContentRootIndex { get; set; }


         public string SubPath { get; set; }
      }
#pragma warning restore CA1812, S3459, S1144
      #endregion
   }
}
