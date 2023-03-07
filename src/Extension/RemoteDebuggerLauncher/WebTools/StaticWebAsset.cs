// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.WebTools
{
   internal class StaticWebAsset
   {
      public StaticWebAsset(string relativeUrl, string path)
      {
         RelativeUrl = relativeUrl;
         Path = path;
      }

      public string RelativeUrl { get; }

      public string Path { get; }
   }
}
