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
      /// <summary>
      /// Combines two strings into a path.
      /// </summary>
      /// <param name="path1">The first path to combine.</param>
      /// <param name="path2">The second path to combine.</param>
      /// <returns>The combined paths.</returns>
      public static string Combine(string path1, string path2)
      {
         var combinedPath = Path.Combine(path1, path2);
         return combinedPath.Replace("\\", "/");
      }

      /// <summary>
      /// Normalizes the supplied path expression.
      /// </summary>
      /// <param name="path">The path to normalize.</param>
      /// <param name="userHomePath">The user home path.</param>
      /// <returns>The normalized path.</returns>
      /// <remarks>The API replaces the  ~ symbol with the supplied user home path.</remarks>
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
      /// Returns a value whether the supplied path value should be normalized.
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
