// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;

namespace RemoteDebuggerLauncher
{
   internal static class UnixPath
   {
      public static string Combine(string path1, string path2)
      {
         var combinedPath = Path.Combine(path1, path2);
         return combinedPath.Replace("\\", "/");
      }
   }
}
