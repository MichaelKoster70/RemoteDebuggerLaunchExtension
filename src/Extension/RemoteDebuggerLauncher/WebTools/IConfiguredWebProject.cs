// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Interface providing web project information on configured level
   /// </summary>
   internal interface IConfiguredWebProject
   {
      /// <summary>
      /// Gets the web root folder from the underlying 'WebRootFolder' MSBUILD property.
      /// </summary>
      string WebRoot { get; }

      string ProjectName { get; }

      IStaticWebAssetsCollectorService StaticWebAssets { get; }
   }
}
