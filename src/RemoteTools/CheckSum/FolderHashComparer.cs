// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteDebuggerLauncher.CheckSum
{
   /// <summary>
   /// Compares the hashes produced by a scan with an expected dictionary of file -> hash
   /// and returns the file paths where the hashes differ or files are missing/inaccessible.
   /// </summary>
   internal sealed class FolderHashComparer : FileScanner
   {
      /// <summary>
      /// Scans <paramref name="startFolder"/> and compares the resulting hashes to <paramref name="expectedHashes"/>.
      /// Returns a list of file paths (relative to <paramref name="startFolder"/>) for which the hashes do
      /// not match, files that are present in expected but missing in the scan, or files that could not be read.
      /// </summary>
      public IReadOnlyList<string> GetMismatchedFiles(string startFolder, IDictionary<string, string> expectedHashes)
      {
         if (string.IsNullOrWhiteSpace(startFolder))
         {
            throw new ArgumentException("startFolder must not be null or empty.", nameof(startFolder));
         }

         if (expectedHashes == null)
         {
            throw new ArgumentNullException(nameof(expectedHashes));
         }

         // perform scan using base class API
         var result = Scan(startFolder);

         var comparer = StringComparer.OrdinalIgnoreCase;
         var mismatches = new HashSet<string>(comparer);

         // Compare scanned files against expected
         foreach (var kv in result.Hashes)
         {
            var path = kv.Key;
            var actualHash = kv.Value;

            if (!expectedHashes.TryGetValue(path, out var expectedHash) ||
                !string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            {
               _ = mismatches.Add(path);
            }
         }

         // Any expected file that wasn't scanned is a mismatch
         foreach (var expectedPath in expectedHashes.Keys.Where(expectedPath => !result.Hashes.ContainsKey(expectedPath)))
         {
            _ = mismatches.Add(expectedPath);
         }

         // Include inaccessible files from the scan
         if (result.InaccessibleFiles != null)
         {
            foreach (var inaccessible in result.InaccessibleFiles)
            {
               _ = mismatches.Add(inaccessible);
            }
         }

         return new List<string>(mismatches);
      }
   }
}
