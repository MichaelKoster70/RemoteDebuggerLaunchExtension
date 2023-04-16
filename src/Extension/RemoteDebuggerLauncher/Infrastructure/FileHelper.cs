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
   /// Utility class providing file support functions.
   /// </summary>
   internal static class FileHelper
   {
      /// <summary>
      /// Returns a value whether the supplied file contains a given text.
      /// </summary>
      /// <param name="path">he file to search.</param>
      /// <param name="contents">The content to match.</param>
      /// <returns><c>true</c> if file contains the text; else <c>false</c>.</returns>
      public static bool ContainsText(string path, string contents)
      {
         if (File.Exists(path))
         {
            var text = File.ReadAllText(path);
            return text.Contains(contents);
         }

         return false;
      }
   }
}
