// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RemoteDebuggerLauncher.CheckSum
{
   /// <summary>
   /// Compares the hashes produced by a scan with an expected dictionary of file -> hash
   /// and returns the file paths where the hashes differ or files are missing.
   /// </summary>
   public sealed class DirectoryScannerComparer : DirectoryScanner
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="DirectoryScannerComparer"/> class with the specified root folder.
      /// </summary>
      /// <param name="rootFolder">The root folder path to be used as the base directory for scanning and comparison operations. Must not be null or empty.</param>
      public DirectoryScannerComparer(string rootFolder) : base(rootFolder)
      {
         //EMPTY_BODY
      }

      /// <summary>
      /// Scans <paramref name="startFolder"/> and compares the resulting hashes to <paramref name="actualHashes"/>.
      /// Returns a list of file paths (relative to <paramref name="startFolder"/>) for which the hashes do
      /// not match, files that are present in expected but missing in the scan, or files that could not be read.
      /// </summary>
      public (IReadOnlyList<string> FilesToCopy, IReadOnlyList<string> FilesToDelete) GetMismatchedFiles(string actualHashesJson)
      {
         if (string.IsNullOrWhiteSpace(actualHashesJson))
         {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(actualHashesJson));
         }
         var actualHashes = JsonConvert.DeserializeObject<Dictionary<string, string>>(actualHashesJson);
         return GetMismatchedFiles(actualHashes);
      }

      /// <summary>
      /// Scans <paramref name="startFolder"/> and compares the resulting hashes to <paramref name="actualHashes"/>.
      /// Returns a list of file paths (relative to <paramref name="startFolder"/>) for which the hashes do
      /// not match, files that are present in expected but missing in the scan, or files that could not be read.
      /// </summary>
      /// <param name="actualHashes">The dictionary holding the actual file hashes</param>
      public (IReadOnlyList<string> FilesToCopy, IReadOnlyList<string> FilesToDelete) GetMismatchedFiles(IDictionary<string, string> actualHashes)
      {
         if (actualHashes == null)
         {
            throw new ArgumentNullException(nameof(actualHashes));
         }

         // perform scan using base class API
         var result = ComputeHashes();

         return CollectMismatches(actualHashes, result.Hashes);
      }

      /// <summary>
      /// Compares <paramref name="actualHashes"/> and <paramref name="expectedHashes"/>.
      /// Returns a tuple where:
      /// - Mismatches: files whose hashes differ or expected files that are missing from the scan.
      /// - ExtraFiles: files that are present in <paramref name="actualHashes"/> but not in <paramref name="expectedHashes"/>.
      /// </summary>
      private static (IReadOnlyList<string> FilesToCopy, IReadOnlyList<string> FilesToDelete) CollectMismatches(IDictionary<string, string> actualHashes, IDictionary<string, string> expectedHashes)
      {
         var comparer = StringComparer.OrdinalIgnoreCase;
         var filesToCopy = new HashSet<string>(comparer);
         var filesToDelete = new HashSet<string>(comparer);

         // 1) Files that exist in both places but whose hashes differ (to copy), or are present in actualHashes but not in expectedHashes (to delete).
         foreach (var kv in actualHashes)
         {
            var path = kv.Key;
            var actualHash = kv.Value;

            if (expectedHashes.TryGetValue(path, out var expectedHash))
            {
               if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
               {
                  _ = filesToCopy.Add(path);
               }
            }
            else
            {
               // present in actual but not expected
               _ = filesToDelete.Add(path);
            }
         }

         // 2) Any expected file that wasn't scanned is a new file
         foreach (var expectedPath in expectedHashes.Keys.Where(expectedPath => !actualHashes.ContainsKey(expectedPath)))
         {
            _ = filesToCopy.Add(expectedPath);
         }

         return (new List<string>(filesToCopy), new List<string>(filesToDelete));
      }
   }
}
