// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Interface defining the static web assets collector service.
   /// Read the assets from the build output metadata files.
   /// </summary>
   internal interface IStaticWebAssetsCollectorService
   {
      Task<IReadOnlyDictionary<string, string>> GetPathMappingsAsync();

      Task<IReadOnlyList<StaticWebAsset>> GetAllStaticAssetsAsync();
   }
}
