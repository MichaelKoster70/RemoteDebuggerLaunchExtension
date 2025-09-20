// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RemoteDebuggerLauncher.CheckSum
{
   /// <summary>
   /// Scans a directory tree and computes SHA256 hashes for each file.
   /// </summary>
   public class DirectoryScanner
   {
      private readonly string rootFolder;

      public DirectoryScanner(string rootFolder)
      {
         if (string.IsNullOrWhiteSpace(rootFolder))
         {
            throw new ArgumentException("startFolder must not be null or empty.", nameof(rootFolder));
         }

         if (!Directory.Exists(rootFolder))
         {
            throw new DirectoryNotFoundException($"Start folder does not exist: {rootFolder}");
         }

         this.rootFolder = rootFolder;
      }

      /// <summary>
      /// Traverse the filesystem starting at the instance's <see cref="rootFolder"/> and compute SHA256 for each file.
      /// The returned <see cref="ScanResult"/> contains a dictionary that maps the full file path to the hex-encoded SHA256 hash
      /// and a list of files that could not be accessed/read during the scan.
      /// Files or folders that cannot be accessed are skipped.
      /// </summary>
      /// <returns>A <see cref="ScanResult"/> with file path -> SHA256 hex string and inaccessible files list.</returns>
      /// <remarks>Assumes the caller guarantees that <paramref name="fullPath"/> begins with <see cref="rootFolder"/>.</remarks>
      protected ScanResult ComputeHashes()
      {
         // Create the ScanResult that will be populated during the scan
         var scanResult = new ScanResult();

         // First try the simpler/fast approach: enumerate all files using AllDirectories.
         // This is efficient but will throw if a directory cannot be accessed. In that case
         // we fall back to a robust manual traversal that records inaccessible entries.
         try
         {
            foreach (var file in Directory.EnumerateFiles(rootFolder, "*", SearchOption.AllDirectories))
            {
               ComputeHashForFile(file, scanResult);
            }

            return scanResult;
         }
         catch (UnauthorizedAccessException)
         {
            // fall through to robust traversal
         }

         // Robust stack-based traversal
         var dirs = new Stack<string>();
         dirs.Push(rootFolder);

         while (dirs.Count > 0)
         {
            var current = dirs.Pop();

            string[] subdirs;
            try
            {
               subdirs = Directory.GetDirectories(current);

               // push subdirectories onto stack; if Directory.GetDirectories throws we skip this directory
               if (subdirs != null)
               {
                  foreach (var d in subdirs)
                  {
                     dirs.Push(d);
                  }
               }
            }
            catch (UnauthorizedAccessException)
            {
               // skip directories we cannot access
               continue;
            }

            string[] files;
            try
            {
               files = Directory.GetFiles(current);

               // enumerate files and compute hashes; if Directory.GetFiles throws we skip files in this directory
               if (files != null)
               {
                  foreach (var file in files)
                  {
                     ComputeHashForFile(file, scanResult);
                  }
               }
            }
            catch (UnauthorizedAccessException)
            {
               // skip files in directories we cannot access
            }
         }

         return scanResult;
      }

      private void ComputeHashForFile(string file, ScanResult result)
      {
         try
         {
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
               using (var sha = SHA256.Create())
               {
                  var hash = sha.ComputeHash(stream);
                  var hex = ToHex(hash);
                  var relative = GetRelativePath(file);
                  result.AddHash(relative, hex);
               }
            }
         }
         catch (IOException)
         {
            // record files that cannot be read
            var relative = GetRelativePath(file);
            result.AddInaccessible(relative);
         }
         catch (UnauthorizedAccessException)
         {
            // record files that cannot be read
            var relative = GetRelativePath(file);
            result.AddInaccessible(relative);
         }
      }

      /// <summary>
      /// Converts bytes to lower-case hex
      /// </summary>
      /// <param name="bytes">The bytes to convert.</param>
      /// <returns>Hexadecimal string.</returns>
      private static string ToHex(byte[] bytes)
      {
         var sb = new StringBuilder(bytes.Length * 2);
         foreach (var b in bytes)
         {
            _ = sb.Append(b.ToString("x2"));
         }
         return sb.ToString();
      }

      /// <summary>
      /// Returns path of file relative to the provided <paramref name="rootFolder"/>. 
      /// </summary>
      /// <param name="fullPath">The full path to convert.</param>
      /// <returns>The relative path.</returns>
      /// <remarks>Assumes the caller guarantees that <paramref name="fullPath"/> begins with <see cref="rootFolder"/>.</remarks>
      protected string GetRelativePath(string fullPath)
      {
         if (fullPath == null)
         {
            return fullPath;
         }

         var comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

         // Caller guarantees prefix; do a fast substring and trim any leading separators.
         string relative;
         if (fullPath.StartsWith(rootFolder, comparison))
         {
            relative = fullPath.Substring(rootFolder.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
         }
         else
         {
            // Safety fallback
            relative = fullPath;
         }

         // Normalize to Unix-style separators ('/').
         // Replace both primary and alternate separators with forward slash.
         relative = relative.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

         return relative;
      }
   }
}
