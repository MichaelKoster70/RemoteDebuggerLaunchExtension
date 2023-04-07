// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.IO;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class providing directory support functions.
   /// </summary>
   internal static class DirectoryHelper
   {
      /// <summary>
      /// Ensures that the supplied folder exists.
      /// </summary>
      /// <param name="folder">The folder path to validate.</param>
      /// <returns><c>true</c> is successful; else <c>false</c>.</returns>
      internal static bool EnsureExists(string folder)
      {
#pragma warning disable CA1031 // Do not catch general exception types
         try
         {
            if (!Directory.Exists(folder))
            {
               _ = Directory.CreateDirectory(folder);
            }
         }
         catch
         {
            return false;
         }
#pragma warning restore CA1031 // Do not catch general exception types
         return true;
      }
   }
}
