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
   /// Result of a scan operation.
   /// </summary>
   internal sealed class ScanResult
   {
      /// <summary>
      /// Mapping from full file path to SHA256 hex string.
      /// </summary>
      // Make settable and add a public parameterless constructor so JSON serializers
      // (System.Text.Json, Newtonsoft.Json) can create and populate this type.
      public IDictionary<string, string> Hashes { get; set; }

      /// <summary>
      /// List of file paths that could not be accessed/read during the scan.
      /// </summary>
      public IList<string> InaccessibleFiles { get; set; }

      /// <summary>
      /// Parameterless constructor required by some JSON serializers.
      /// Initializes properties with empty collections to preserve original invariants.
      /// </summary>
      public ScanResult()
      {
         Hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
         InaccessibleFiles = new List<string>();
      }

      internal ScanResult(IDictionary<string, string> hashes, IList<string> inaccessibleFiles)
      {
         Hashes = hashes ?? throw new ArgumentNullException(nameof(hashes));
         InaccessibleFiles = inaccessibleFiles ?? throw new ArgumentNullException(nameof(inaccessibleFiles));
      }

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
