// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace RemoteDebuggerLauncher.CheckSum
{
   /// <summary>
   /// Result of a <see cref="DirectoryScanner"/> operation.
   /// </summary>
   public sealed class ScanResult
   {
      /// <summary>
      /// Parameterless constructor required by some JSON serializers.
      /// Initializes properties with empty collections to preserve original invariants.
      /// </summary>
      public ScanResult()
      {
         Hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
         InaccessibleFiles = new List<string>();
      }

      /// <summary>
      /// Mapping from full file path to SHA256 hex string.
      /// </summary>
      public IDictionary<string, string> Hashes { get; }

      /// <summary>
      /// List of file paths that could not be accessed/read during the scan.
      /// </summary>
      public IList<string> InaccessibleFiles { get; }

      /// <summary>
      /// Adds or updates a hash entry for a file path.
      /// </summary>
      /// <param name="filePath">Full file path.</param>
      /// <param name="hexHash">SHA256 hex string.</param>
      public void AddHash(string filePath, string hexHash)
      {
         if (filePath == null)
         {
            throw new ArgumentNullException(nameof(filePath));
         }

         Hashes[filePath] = hexHash ?? throw new ArgumentNullException(nameof(hexHash));
      }

      /// <summary>
      /// Records a file path that could not be accessed/read during the scan.
      /// </summary>
      /// <param name="filePath">Full file path that was inaccessible.</param>
      public void AddInaccessible(string filePath)
      {
         if (filePath == null)
         {
            throw new ArgumentNullException(nameof(filePath));
         }

         InaccessibleFiles.Add(filePath);
      }
   }
}
