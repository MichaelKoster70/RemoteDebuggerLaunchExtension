// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class holding Unix specific path operations.
   /// </summary>
   internal static class UnixPath
   {
      public static string Combine(string path1, string path2)
      {
         var combinedPath = Path.Combine(path1, path2);
         return combinedPath.Replace("\\", "/");
      }

      public static string Normalize(string path, string userHomePath)
      {
         ThrowIf.ArgumentNullOrEmpty(path, nameof(path));
         ThrowIf.ArgumentNullOrEmpty(userHomePath, nameof(userHomePath));

         if (path.StartsWith("~"))
         {
            return path.Replace("~", userHomePath);
         }

         return path;
      }

      /// <summary>
      /// Returns a value whether the supplied path value should be normalized;
      /// </summary>
      /// <param name="path">The path to check.</param>
      /// <returns><c>true</c> if the path begins with the ~ symbol; else <c>false</c>.</returns>
      public static bool ShouldBeNormalized(string path)
      {
         ThrowIf.ArgumentNullOrEmpty(path, nameof(path));

         return path.StartsWith("~");
      }
   }
}
